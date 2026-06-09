using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using Mono.Cecil;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Security.Principal;

/// <summary>
/// Custom Inspector for <see cref="InputConfigSO"/>.
/// Provides a specialized interface for managing Input Map and Action overrides 
/// by resolving GUIDs through a temporary reference to an <see cref="InputActionAsset"/>.
/// </summary>
[CustomEditor(typeof(InputConfigSO))]
public class InputConfigSOEditor : Editor
{
    private InputActionAsset _loaderAsset;
    private TypeVar _loaderAssetInstanceType;
    private SerializedProperty _assetMapListProp;

    private bool _alreadyCreatedArrayElementForAssetMapList;

    //TODO: make SO database for priorities, so we can assign a unique priority value to each action inside of a config

    private EditorCoroutine DuplicatePriorityResearch;
    private static bool _duplicatePrioritySearchActive;
    private HashSet<string> _mapsWithoutAsset = new();

    private static int _activeInstances = 0;

    private void OnEnable()
    {
        _activeInstances++;
        // first active instance wipe the data and rebuild
        if (_activeInstances == 1)
        {
            _actionsPriorities = new();
            _actionsPrioritiesCount = new();

            DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);
        }

        // rebuild dictionary on project changed (this also triggers when some other assets are created/deleted, not only when InputConfigSOs are)
        EditorApplication.projectChanged += RebuildPriorityDictionary;

        // Access the private [SerializeField] list from the target ScriptableObject
        _assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");

        // try load the last used InputActionAsset
        _loaderAsset = serializedObject.FindProperty("_lastUsedInputAsset").objectReferenceValue as InputActionAsset;

        _alreadyCreatedArrayElementForAssetMapList = false;

        // checks whether there are maps not belonging to any InputActionAsset, this happens when the it's deleted
        InputActionAsset[] inputActionAssets = AssetDatabaseUtils.GetAssetsByType<InputActionAsset>();

        _mapsWithoutAsset = new();

        // loop through each map and check if it belongs to any inputActionAsset, if not add it to maps without an asset
        LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        {
            bool found = false;
            foreach (var item in inputActionAssets)
            {
                if (item.FindActionMap(nameOrId: guid) != null)
                {
                    found = true;
                    break;
                }
            }
            if (!found) _mapsWithoutAsset.Add(guid);
        });
    }


    void OnDisable()
    {
        // remove instance
        _activeInstances--;
        // if no active instances clear the dictionaries
        if (_activeInstances == 0)
        {
            _actionsPriorities = null;
            _actionsPrioritiesCount = null;
        }

        EditorApplication.projectChanged -= RebuildPriorityDictionary;

        // remove temporary and unused InputAssetMapList, this happens if user selects InputActionAsset and TypeVar but doesn't add maps
        for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            int mapsCount = inputAssetMapList.FindPropertyRelative("InputMapStructs").arraySize;
            if (mapsCount == 0)
            {
                _assetMapListProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
            }
        }

        // stop research coroutine if still active
        if (DuplicatePriorityResearch != null)
        {
            EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
            DuplicatePriorityResearch = null;
        }
    }

    private int _dictionaryBuildingWaitingDots = 0;
    private int _dictionaryBuildingWaitingDotsMax = 3;
    private float _dictionaryBuildingWaitingDotsDelay = 0.5f;
    private float _dictionaryBuildingWaitingDotsTimeElapsed = 0.0f;

    /// <summary>
    /// Updates the serialized object and draws the custom inspector elements, 
    /// including the loader field and the filtered map list.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // disable GUI while loading priorities
        GUI.enabled = !_duplicatePrioritySearchActive;

        if (_duplicatePrioritySearchActive)
        {
            Debug.Log("POlo");
            string waitingDots = string.Empty;
            for (int i = 0; i < _dictionaryBuildingWaitingDots; i++)
            {
                waitingDots += ".";
            }
            _dictionaryBuildingWaitingDotsTimeElapsed += Time.deltaTime;
            if (_dictionaryBuildingWaitingDotsTimeElapsed >= _dictionaryBuildingWaitingDotsDelay)
            {
                _dictionaryBuildingWaitingDotsTimeElapsed = 0f;
                _dictionaryBuildingWaitingDots++;
                if (_dictionaryBuildingWaitingDots > _dictionaryBuildingWaitingDotsMax)
                {
                    _dictionaryBuildingWaitingDots = 0;
                }
            }
            EditorGUILayout.LabelField("Building priority dictionaries please wait" + waitingDots);
            return;
        }

        serializedObject.Update();
        EditorGUILayout.LabelField("Editor Workspace", EditorStyles.boldLabel);

        // InputActionAsset we are working with
        InputActionAsset previousInputActionAsset = _loaderAsset;
        _loaderAsset = (InputActionAsset)EditorGUILayout.ObjectField("Reference Asset", _loaderAsset, typeof(InputActionAsset), false);

        // asset changed
        if (_loaderAsset != previousInputActionAsset)
        {
            Debug.Log("Asset CHanged");

            // remove temporary and unused InputAssetMapList
            for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
                int mapsCount = inputAssetMapList.FindPropertyRelative("InputMapStructs").arraySize;
                if (mapsCount == 0)
                {
                    _assetMapListProp.DeleteArrayElementAtIndex(i);
                }
            }

            // update last used InputActionAsset
            serializedObject.FindProperty("_lastUsedInputAsset").objectReferenceValue = _loaderAsset;
            serializedObject.ApplyModifiedProperties();

            // allow for new temporaty InputAssetMapList to be created 
            _alreadyCreatedArrayElementForAssetMapList = false;
        }

        // if null print the total number of maps and the maps without an InputActionAsset
        if (_loaderAsset == null)
        {
            EditorGUILayout.LabelField($"There are currently: {GetTotalMaps()} maps");
            EditorGUILayout.Space();
            DrawWarningForMapsWithoutAnAsset();
            EditorGUILayout.HelpBox("Assign an Input Asset to edit overrides.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            return;
        }

        // find the corresponding InputAssetMapList associated to this _loaderAsset
        var assetMapGuids = _loaderAsset.actionMaps.Select(m => m.id.ToString()).ToHashSet();
        int assetMapListIndex = -1;

        LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        {
            if (assetMapGuids.Contains(guid))
            {
                assetMapListIndex = inputAssetMapListIndex;
                _alreadyCreatedArrayElementForAssetMapList = true;
            }
        });

        // no corresponding was found
        // create a new element
        if (assetMapListIndex == -1)
        {
            if (!_alreadyCreatedArrayElementForAssetMapList)
            {
                assetMapListIndex = _assetMapListProp.arraySize;
                _alreadyCreatedArrayElementForAssetMapList = true;
                _assetMapListProp.InsertArrayElementAtIndex(assetMapListIndex);
                _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
                _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("InputMapStructs").ClearArray();

                _assetMapListProp.serializedObject.ApplyModifiedProperties();
                _assetMapListProp.serializedObject.Update();
            }
            else
            {
                assetMapListIndex = _assetMapListProp.arraySize - 1;
            }
        }

        // object field to update TypeVar
        _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = (TypeVar)EditorGUILayout.ObjectField("TypeVar input c# script", _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue, typeof(TypeVar), false);
        _loaderAssetInstanceType = (TypeVar)_assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue;

        // if TypeVar is null we can't proceed
        if (_assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Assign a TypeVar to associate to the Reference Asset to edit overrides.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();

        DrawWarningForMapsWithoutAnAsset();

        EditorGUILayout.Space();

        // Add Map Button
        DrawAddMapMenu(assetMapListIndex);

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Priority Dictionary"))
        {
            if (DuplicatePriorityResearch != null)
            {
                EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
                DuplicatePriorityResearch = null;
            }
            DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);
        }

        // Draw Filtered Maps
        // Only render maps that exist in this InputConfigSO and that are present in the assigned loader asset
        // this is the filter step for ensuring we can only add/show maps that are present in this loader asset

        LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        {
            SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative(relativePropertyPath: "InputMapStructs");
            SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(mapIndex);
            if (assetMapGuids.Contains(guid))
            {
                DrawMapFoldout(mapElem, inputAssetMapListIndex, mapIndex);
            }
        });

        serializedObject.ApplyModifiedProperties();
    }

    // <guid,<InputConfigSO, propertyPath>>
    private static Dictionary<string, HashSet<InputConfigSO>> _actionsPriorities = new();
    // <guid,<priority, priorityCount>>
    private static Dictionary<string, Dictionary<int, int>> _actionsPrioritiesCount = new();

    public static Dictionary<string, HashSet<InputConfigSO>> ActionsPriorities => _actionsPriorities;
    public static Dictionary<string, Dictionary<int, int>> ActionsPrioritiesCount => _actionsPrioritiesCount;

    private void RebuildPriorityDictionary()
    {
        if (_duplicatePrioritySearchActive) return;
        _actionsPriorities = new();
        _actionsPrioritiesCount = new();

        if (DuplicatePriorityResearch != null)
        {
            EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
            DuplicatePriorityResearch = null;
        }
        DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);
    }

    public static bool IsThereAnyPriorityConflict(string guid)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return false;

        foreach (var priorityCounts in _actionsPrioritiesCount[guid])
        {
            if (priorityCounts.Value > 1)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetPriorityCountForAction(string guid, int priority)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return -1;
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return -1;

        return _actionsPrioritiesCount[guid][priority];
    }

    public static void AddPriority(string guid, int priority, string propertyPath, InputConfigSO inputConfig)
    {
        if (!_actionsPriorities.ContainsKey(guid)) _actionsPriorities[guid] = new();
        if (!_actionsPrioritiesCount.ContainsKey(guid)) _actionsPrioritiesCount[guid] = new();

        if (!_actionsPriorities[guid].Contains(inputConfig)) _actionsPriorities[guid].Add(inputConfig);
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) _actionsPrioritiesCount[guid][priority] = 0;
        _actionsPrioritiesCount[guid][priority]++;
    }

    public static void RemovePriority(string guid, int priority, InputConfigSO inputConfig)
    {
        if (!_actionsPriorities.ContainsKey(guid)) return;
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return;
        if (!_actionsPriorities[guid].Contains(inputConfig)) return;
        _actionsPriorities[guid].Remove(inputConfig);
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return;
        _actionsPrioritiesCount[guid][priority]--;
    }

    public static void UpdatePriority(string guid, int oldPriority, int newPriority, string propertyPath, InputConfigSO inputConfig)
    {
        if (oldPriority == newPriority) return;
        RemovePriority(guid, oldPriority, inputConfig);
        AddPriority(guid, newPriority, propertyPath, inputConfig);
    }

    private IEnumerator DuplicatePrioritySearchCoroutine()
    {
        _duplicatePrioritySearchActive = true;
        _actionsPriorities.Clear();
        _actionsPrioritiesCount.Clear();

        var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>();

        HashSet<int> unavailablePriorities = new();
        // find all configs where action is found
        for (int i = allConfigs.Length - 1; i >= 0; i--)
        {
            // bool foundAction = false;
            InputConfigSO config = allConfigs.ElementAt(i);
            SerializedObject serializedConfig = new(config);

            var inputAssetMaps = serializedConfig.FindProperty("_inputAssetMaps");

            for (int j = 0; j < inputAssetMaps.arraySize; j++)
            {
                TypeVar typeVar = inputAssetMaps.GetArrayElementAtIndex(j).FindPropertyRelative("AssetType").objectReferenceValue as TypeVar;
                if (typeVar == null || typeVar.Type == null) continue;

                SerializedProperty inputMapStructs = inputAssetMaps.GetArrayElementAtIndex(j).FindPropertyRelative("InputMapStructs");
                for (int k = 0; k < inputMapStructs.arraySize; k++)
                {
                    SerializedProperty inputActionStructs = inputMapStructs.GetArrayElementAtIndex(k).FindPropertyRelative("InputActionStructs");
                    for (int l = 0; l < inputActionStructs.arraySize; l++)
                    {
                        string guid = inputActionStructs.GetArrayElementAtIndex(l).FindPropertyRelative("Guid").stringValue;

                        // build priority dictionary
                        int priority = inputActionStructs.GetArrayElementAtIndex(l).FindPropertyRelative("Priority").intValue;
                        if (!_actionsPrioritiesCount.ContainsKey(guid)) _actionsPrioritiesCount[guid] = new();
                        var priorityCount = _actionsPrioritiesCount[guid];
                        if (!priorityCount.ContainsKey(priority)) priorityCount[priority] = 0;
                        priorityCount[priority]++;

                        if (!_actionsPriorities.ContainsKey(guid)) _actionsPriorities[guid] = new();
                        var inputConfigs = _actionsPriorities[guid];
                        inputConfigs.Add(config);
                    }
                }
            }
        }

        _duplicatePrioritySearchActive = false;
        yield break;
    }

    private enum PriorityAvailabilityEnum
    {
        /// <summary>
        /// Selected priority doesn't conflict, but there are there configs that do
        /// </summary>
        SELF_AVAILABLE,
        /// <summary>
        /// Selected priority conflicts with at least another config
        /// </summary>
        SELF_CONFLICT,
        /// <summary>
        /// All of the configs don't conflict with each other
        /// </summary>
        NO_CONFLICT
    }

    private PriorityAvailabilityEnum IsPriorityAvailable(string guid, int requestedPriority)
    {
        if (_actionsPrioritiesCount.Count == 0) return PriorityAvailabilityEnum.NO_CONFLICT;
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return PriorityAvailabilityEnum.NO_CONFLICT;

        int priorityCount = GetPriorityCountForAction(guid, requestedPriority);
        bool isThereAnyPriorityConflict = IsThereAnyPriorityConflict(guid);
        if (priorityCount == -1 || priorityCount - 1 > 0)
        {
            return PriorityAvailabilityEnum.SELF_CONFLICT;
        }
        else
        {
            if (isThereAnyPriorityConflict)
            {
                return PriorityAvailabilityEnum.SELF_AVAILABLE;
            }
            else
            {
                return PriorityAvailabilityEnum.NO_CONFLICT;
            }
        }
    }

    public object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;
        // The 'serializedObject.targetObject' is the Component or ScriptableObject
        var targetObj = prop.serializedObject.targetObject;

        // We then use reflection to find the specific field the property represents
        var field = targetObj.GetType().GetField(prop.name,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        return field?.GetValue(targetObj);
    }

    private int GetTotalMaps()
    {
        int totalMaps = 0;
        for (int i = 0; i < _assetMapListProp.arraySize; i++)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");
            totalMaps += mapStructList.arraySize;
        }
        return totalMaps;
    }

    private void DrawWarningForMapsWithoutAnAsset()
    {
        if (_mapsWithoutAsset.Count == 0) return;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.HelpBox($"There are {_mapsWithoutAsset.Count} maps with missing InputActionAsset", MessageType.Warning);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                LoopThroughMapsReverse((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
                {
                    SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");
                    if (_mapsWithoutAsset.Contains(guid))
                    {
                        mapStructList.DeleteArrayElementAtIndex(mapIndex);
                        _mapsWithoutAsset.Remove(guid);
                    }
                });
            }
        }
    }

    private void LoopThroughMaps(Action<string, SerializedProperty, int, int> action)
    {
        for (int i = 0; i < _assetMapListProp.arraySize; i++)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");

            for (int j = 0; j < mapStructList.arraySize; j++)
            {
                SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(j);
                string guid = mapElem.FindPropertyRelative("Guid").stringValue;
                action?.Invoke(guid, inputAssetMapList, i, j);
            }
        }
    }

    private void LoopThroughMapsReverse(Action<string, SerializedProperty, int, int> action)
    {
        for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");

            for (int j = mapStructList.arraySize - 1; j >= 0; j--)
            {
                SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(j);
                string guid = mapElem.FindPropertyRelative("Guid").stringValue;
                action?.Invoke(guid, inputAssetMapList, i, j);
            }
        }
    }

    /// <summary>
    /// Draws a foldout for a specific Input Map, displaying its action overrides 
    /// and providing options to add or remove overrides.
    /// </summary>
    private void DrawMapFoldout(SerializedProperty mapProp, int inputAssetMapListIndex, int mapIndex)
    {
        string guid = mapProp.FindPropertyRelative("Guid").stringValue;
        var assetMap = _loaderAsset.actionMaps.First(m => m.id.ToString() == guid);

        EditorGUILayout.BeginVertical("helpbox");

        // Header Row with Map Name and Remove Button
        EditorGUILayout.BeginHorizontal();
        mapProp.isExpanded = EditorGUILayout.Foldout(mapProp.isExpanded, $"Map: {assetMap.name}", true);

        // Matches the field name in InputMapStruct
        SerializedProperty actionsList = mapProp.FindPropertyRelative("InputActionStructs");
        InputConfigSO inputConfig = target as InputConfigSO;
        using (new EditorGUI.DisabledScope(actionsList.arraySize == assetMap.actions.Count))
        {
            // adds all of the missing actions to the map
            if (GUILayout.Button(new GUIContent("Add all", "Adds all of the actions of the map"), GUILayout.Width(100)))
            {
                // avoids adding to the menu the already added actions
                var existingGuids = GetExistingGuids(actionsList, "Guid");
                foreach (var action in assetMap.actions)
                {
                    string actionGuid = action.id.ToString();
                    if (existingGuids.Contains(actionGuid)) continue;

                    // if action isn't present already add it
                    int index = actionsList.arraySize;
                    actionsList.InsertArrayElementAtIndex(index);
                    var newAction = actionsList.GetArrayElementAtIndex(index);
                    newAction.FindPropertyRelative("Guid").stringValue = actionGuid;
                    newAction.FindPropertyRelative("Enabled").boolValue = true;
                    newAction.FindPropertyRelative("Priority").intValue = 0;

                    // and update priority dictionaries
                    AddPriority(actionGuid, 0, newAction.propertyPath, inputConfig);

                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {
            SerializedProperty inputActionStructs = mapProp.FindPropertyRelative("InputActionStructs");

            for (int i = 0; i < inputActionStructs.arraySize; i++)
            {
                SerializedProperty actionProp = inputActionStructs.GetArrayElementAtIndex(i);
                int priority = actionProp.FindPropertyRelative("Priority").intValue;
                RemovePriority(actionProp.FindPropertyRelative("Guid").stringValue, priority, inputConfig);
            }

            _assetMapListProp.GetArrayElementAtIndex(inputAssetMapListIndex).FindPropertyRelative("InputMapStructs").DeleteArrayElementAtIndex(mapIndex);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        // Actions Section
        if (mapProp.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Column Header Labels
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View all", EditorStyles.miniBoldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Action Name", EditorStyles.miniBoldLabel, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
            EditorGUILayout.LabelField("On", EditorStyles.miniBoldLabel, GUILayout.Width(40));
            EditorGUILayout.LabelField("Priority", EditorStyles.miniBoldLabel, GUILayout.MinWidth(60), GUILayout.MaxWidth(100));
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            // Draw each action row with its properties and a remove button
            for (int j = 0; j < actionsList.arraySize; j++)
            {
                SerializedProperty actionElem = actionsList.GetArrayElementAtIndex(j);
                DrawActionRow(actionElem, assetMap, actionsList, j);
            }

            // Add Action Button at the end of the list
            DrawAddActionMenu(actionsList, assetMap);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Renders a horizontal row for an individual action override.
    /// </summary>
    private void DrawActionRow(SerializedProperty actionProp, InputActionMap assetMap, SerializedProperty list, int index)
    {
        string actionGUID = actionProp.FindPropertyRelative("Guid").stringValue;
        var action = assetMap.actions.FirstOrDefault(a => a.id.ToString() == actionGUID);

        int oldPriority = actionProp.FindPropertyRelative("Priority").intValue;

        // draw the action name instead of the guid, then the enabled toggle, the priority field and a remove button
        EditorGUILayout.BeginHorizontal();

        // view all button
        string viewAllButtonText = string.Empty;
        string viewAllButtonTooltip = string.Empty;
        PriorityAvailabilityEnum priorityAvailable = IsPriorityAvailable(actionGUID, oldPriority);

        Color guiColor = GUI.color;
        switch (priorityAvailable)
        {
            case PriorityAvailabilityEnum.SELF_AVAILABLE:
                viewAllButtonText = "View All";
                viewAllButtonTooltip = "This priority doesn't conflict with other configs, but there are conflicts to resolve";
                GUI.color = Color.Lerp(guiColor, Color.yellow, 0.4f);
                break;
            case PriorityAvailabilityEnum.SELF_CONFLICT:
                viewAllButtonText = "Fix Priority";
                viewAllButtonTooltip = "This action's priority conflicts with other configs";
                GUI.color = Color.Lerp(Color.red, Color.yellow, 0.8f);
                break;
            case PriorityAvailabilityEnum.NO_CONFLICT:
                viewAllButtonText = "View All";
                viewAllButtonTooltip = "No conflicts for this action";
                GUI.color = Color.paleGreen;
                break;
        }

        if (GUILayout.Button(new GUIContent(viewAllButtonText, viewAllButtonTooltip), GUILayout.Width(100)))
        {
            if (!_actionsPriorities.ContainsKey(actionGUID))
            {
                Debug.LogWarning("ActionGUID isn't inside the Priority Dictionary, click the button to update it");
            }
            else
            {
                Rect buttonRect = GUILayoutUtility.GetLastRect();
                Dictionary<InputConfigSO, string> actionPrioritiesPaths = new();
                foreach (var item in _actionsPriorities[actionGUID])
                {
                    actionPrioritiesPaths[item] = GetPriorityPropertyPath(actionGUID, item);
                }
                PopupWindow.Show(buttonRect, new PopupPriorityHelper(actionPrioritiesPaths, action?.name ?? "Unknown"));
            }
        }

        GUI.color = guiColor;

        // action name
        EditorGUILayout.LabelField(action?.name ?? "Unknown", GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
        // enabled status

        // check if anything changes
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));
        // priority
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.MinWidth(60), GUILayout.MaxWidth(100));

        int newPriority = actionProp.FindPropertyRelative("Priority").intValue;
        // remove button
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
            list.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }
        // if something changed check if priority did and in that case update
        else if (EditorGUI.EndChangeCheck())
        {
            if (oldPriority != newPriority)
                UpdatePriority(actionGUID, oldPriority, newPriority, actionProp.propertyPath, (InputConfigSO)target);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Displays a context menu allowing the user to add a new map override 
    /// from the assigned loader asset, ensuring the new map entry is cleared of cloned data.
    /// </summary>
    private void DrawAddMapMenu(int assetMapListIndex)
    {
        if (GUILayout.Button("Add Map Override..."))
        {
            GenericMenu menu = new();

            SerializedProperty mapList = _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("InputMapStructs");
            // get the existing map guids in this so to avoid adding duplicates to the menu
            var existingGuids = GetExistingGuids(mapList, "Guid");

            // loop through the maps of the loader asset and only add to the menu the ones that are not already present in this SO
            foreach (var map in _loaderAsset.actionMaps)
            {
                string mapGuid = map.id.ToString();
                if (existingGuids.Contains(mapGuid)) continue;

                // add item to menu, when item is clicked we add to map list a new element with the guid of the selected map
                menu.AddItem(new GUIContent(map.name), false, () =>
                {
                    // add new item with the next index available at the end of the list
                    int index = mapList.arraySize;
                    mapList.InsertArrayElementAtIndex(index);

                    // set the guid of the new map element to the guid of the current map
                    var newMap = mapList.GetArrayElementAtIndex(index);
                    newMap.FindPropertyRelative("Guid").stringValue = mapGuid;

                    // reset the cloned action list to ensure a clean slate, otherwise it would try to copy the actions of the last element in the list
                    var newActionsList = newMap.FindPropertyRelative("InputActionStructs");
                    newActionsList.ClearArray();

                    // expand by default
                    newMap.isExpanded = true;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No more maps to add"));
            }
            menu.ShowAsContext();
        }
    }

    /// <summary>
    /// Displays a context menu to add specific action overrides to a map.
    /// </summary>
    private void DrawAddActionMenu(SerializedProperty actionsList, InputActionMap assetMap)
    {
        if (GUILayout.Button("Add Action Override...", GUILayout.Width(160)))
        {
            GenericMenu menu = new GenericMenu();
            // avoids adding to the menu the already added actions
            var existingGuids = GetExistingGuids(actionsList, "Guid");

            InputConfigSO inputConfig = (InputConfigSO)target;

            foreach (var action in assetMap.actions)
            {
                string actionGuid = action.id.ToString();
                if (existingGuids.Contains(actionGuid)) continue;

                menu.AddItem(new GUIContent(action.name), false, () =>
                {
                    int index = actionsList.arraySize;
                    actionsList.InsertArrayElementAtIndex(index);
                    var newAction = actionsList.GetArrayElementAtIndex(index);
                    newAction.FindPropertyRelative("Guid").stringValue = actionGuid;
                    newAction.FindPropertyRelative("Enabled").boolValue = true;

                    int priorityValue = 0;

                    newAction.FindPropertyRelative("Priority").intValue = priorityValue;
                    AddPriority(actionGuid, 0, newAction.propertyPath, inputConfig);

                    serializedObject.ApplyModifiedProperties();
                });
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No more actions to add"));
            }
            menu.ShowAsContext();
        }
    }

    /// <summary>
    /// Scans a serialized list to retrieve all unique GUIDs currently stored.
    /// </summary>
    private HashSet<string> GetExistingGuids(SerializedProperty list, string relativePath)
    {
        var set = new HashSet<string>();
        for (int i = 0; i < list.arraySize; i++)
        {
            set.Add(list.GetArrayElementAtIndex(i).FindPropertyRelative(relativePath).stringValue);
        }
        return set;
    }

    private void FindAllConfigsOverridingAction(string guid)
    {
        var configs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>();
    }

    private string GetPriorityPropertyPath(string actionGuid, InputConfigSO inputConfig)
    {
        // bool foundAction = false;
        SerializedObject serializedConfig = new(inputConfig);

        var inputAssetMaps = serializedConfig.FindProperty("_inputAssetMaps");

        for (int j = 0; j < inputAssetMaps.arraySize; j++)
        {
            SerializedProperty inputAssetMapList = inputAssetMaps.GetArrayElementAtIndex(j);
            TypeVar typeVar = inputAssetMapList.FindPropertyRelative("AssetType").objectReferenceValue as TypeVar;
            if (!typeVar || typeVar.Type == null) continue;

            SerializedProperty inputMapStructs = inputAssetMapList.FindPropertyRelative("InputMapStructs");
            for (int k = 0; k < inputMapStructs.arraySize; k++)
            {
                SerializedProperty inputActionStructs = inputMapStructs.GetArrayElementAtIndex(k).FindPropertyRelative("InputActionStructs");
                for (int l = 0; l < inputActionStructs.arraySize; l++)
                {
                    string guid = inputActionStructs.GetArrayElementAtIndex(l).FindPropertyRelative("Guid").stringValue;
                    if (guid == actionGuid)
                    {
                        return inputActionStructs.GetArrayElementAtIndex(l).propertyPath;
                    }
                }
            }
        }
        // for (int i = 0; i < _assetMapListProp.arraySize; i++)
        // {
        //     SerializedProperty inputAssetMapLists = _assetMapListProp.GetArrayElementAtIndex(i).FindPropertyRelative("");
        //     for (int j = 0; j < inputAssetMapLists.arraySize; j++)
        //     {
        //         SerializedProperty inputAssetMapList = inputAssetMapLists.GetArrayElementAtIndex(j);
        //         TypeVar typeVar = inputAssetMapList.FindPropertyRelative("AssetType").objectReferenceValue as TypeVar;
        //         if (typeVar || typeVar.Type == null) continue;

        //         SerializedProperty inputMapStructs = inputAssetMapList.FindPropertyRelative("InputMapStructs");
        //         for (int k = 0; k < inputMapStructs.arraySize; k++)
        //         {
        //             SerializedProperty inputActionStructs = inputMapStructs.FindPropertyRelative("InputActionStructs");
        //             for (int l = 0; l < inputActionStructs.arraySize; l++)
        //             {
        //                 string guid = inputActionStructs.GetArrayElementAtIndex(l).FindPropertyRelative("Guid").stringValue;
        //                 if (guid == actionGuid)
        //                 {
        //                     return inputActionStructs.GetArrayElementAtIndex(l).propertyPath;
        //                 }
        //             }
        //         }
        //     }
        // }

        return null;
    }
}