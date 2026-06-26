using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

using PriorityAvailabilityEnum = InputConfigPriorityCache.PriorityAvailabilityEnum;

/// <summary>
/// Custom Inspector for <see cref="InputConfigSO"/>.
/// Provides a specialized interface for managing Input Map and Action overrides 
/// by resolving GUIDs through a temporary reference to an <see cref="InputActionAsset"/>.
/// </summary>
[CustomEditor(typeof(InputConfigSO))]
public class InputConfigSOEditor : Editor
{
    //TODO: maybe add a string displayName to actionInputStruct, which is an override of the name, so for instance E would normally be Interact, but maybe we want something more specific like look, open
    // this would become a tabbed window, one for priority, so just like it is now, one for overriding the names, also the "Press @BUTTON to interact"
    private InputActionAsset _loaderAsset;
    private TypeVar _loaderAssetInstanceType;
    private SerializedProperty _assetMapListProp;

    private bool _alreadyCreatedArrayElementForAssetMapList;

    private HashSet<string> _mapsWithoutAsset = new();
    private HashSet<string> _actionsOrphan = new();
    private HashSet<string> _bindingsOrphan = new();

    private readonly double _rebuildDelay = 0.5f;
    private double _rebuildDeadline = -1f;

    private void OnEnable()
    {
        EditorApplication.update += PriorityChangeCheck;
        InputConfigPriorityCache.OnRebuildCompleted += RefreshInspector;

        // Access the private [SerializeField] list from the target ScriptableObject
        _assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");

        // try load the last used InputActionAsset
        _loaderAsset = serializedObject.FindProperty("_lastUsedInputAsset").objectReferenceValue as InputActionAsset;

        _alreadyCreatedArrayElementForAssetMapList = false;

        // checks whether there are maps not belonging to any InputActionAsset, this happens when the it's deleted
        InputActionAsset[] inputActionAssets = AssetDatabaseUtils.GetAssetsByType<InputActionAsset>();

        _mapsWithoutAsset = new();

        InputConfigSO inputConfigSO = target as InputConfigSO;

        InputMapStruct[] maps = inputConfigSO.GetInputAssetMaps().SelectMany(c => c.InputMapStructs).ToArray();
        InputActionEntry[] actions = maps.SelectMany(c => c.InputActionEntries).ToArray();
        BindingPromptData[] bindings = actions.SelectMany(c => c.PromptSchemes).SelectMany(c => c.Prompts).ToArray();


        //TODO: populate the hashsets so when drawing an a element we know if it's valid or orphane and in that case there should be some visual feedback
        // like red or some, or for instance the button remove should be red or shi
        // also a remove all orphanes button a the top could be a thing
        foreach (var item in inputActionAssets)
        {
            foreach (var map in maps)
            {
                actions = map.InputActionEntries.Select(c => c.);
            }
            // if (item.FindActionMap(nameOrId: guid) != null)
            // {
            //     break;
            // }
        }

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
                item.FindAction
            }
            if (!found) _mapsWithoutAsset.Add(guid);
        });
    }

    void OnDisable()
    {
        EditorApplication.update -= PriorityChangeCheck;
        InputConfigPriorityCache.OnRebuildCompleted -= RefreshInspector;

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
    }

    private int _dictionaryBuildingWaitingDots = 0;
    private int _dictionaryBuildingWaitingDotsMax = 3;
    private float _dictionaryBuildingWaitingDotsDelay = 0.5f;
    private float _dictionaryBuildingWaitingDotsTimeElapsed = 0.0f;

    private void RefreshInspector()
    {
        Repaint();
    }

    /// <summary>
    /// Updates the serialized object and draws the custom inspector elements, 
    /// including the loader field and the filtered map list.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // disable GUI while loading priorities
        GUI.enabled = !InputConfigPriorityCache.DuplicatePrioritySearchActive;

        //TODO: can cause some Flickering, might just stick to only disabling the ui, should be fine without returning
        if (InputConfigPriorityCache.DuplicatePrioritySearchActive)
        {
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
        if (_loaderAssetInstanceType == null)
        {
            EditorGUILayout.HelpBox("Assign a TypeVar to associate to the Reference Asset to edit overrides.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        else if (_loaderAssetInstanceType.Type == null)
        {
            Debug.LogWarning("Assign a TypeVar with a Type to associate to the Reference Asset to edit overrides.");
            _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            return;
        }
        else if (!typeof(IInputActionCollection2).IsAssignableFrom(_loaderAssetInstanceType.Type))
        {
            Debug.LogWarning("Assign a TypeVar with Type being the C# generated script of an InputActionAsset to associate to the Reference Asset to edit overrides.");
            _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            return;
        }

        //TODO: might want to check TypeVar.Type since it should only be IInputActionCollection2

        EditorGUILayout.Space();

        DrawWarningForMapsWithoutAnAsset();

        EditorGUILayout.Space();

        // Add Map Button
        DrawAddMapMenu(assetMapListIndex);

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Priority Dictionary"))
        {
            InputConfigPriorityCache.RebuildPriorityDictionary();
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

    private void PriorityChangeCheck()
    {
        if (_rebuildDeadline > 0 && EditorApplication.timeSinceStartup > _rebuildDeadline)
        {
            _rebuildDeadline = -1;
            InputConfigPriorityCache.RebuildPriorityDictionary();
            // Debug.Log("Rebuild from debounce " + name);
        }
    }

    // <guid,HashSet<InputConfigSO>>
    // private static Dictionary<string, HashSet<InputConfigSO>> _actionsPriorities = new();
    // // <guid,<priority, priorityCount>>
    // private static Dictionary<string, Dictionary<int, int>> _actionsPrioritiesCount = new();

    // public static Dictionary<string, HashSet<InputConfigSO>> ActionsPriorities => _actionsPriorities;
    // public static Dictionary<string, Dictionary<int, int>> ActionsPrioritiesCount => _actionsPrioritiesCount;

    // public static void RebuildPriorityDictionary()
    // {
    //     _activeInstances.RemoveWhere(instance => instance == null);

    //     Debug.Log("Rebuilding " + _activeInstances.Count);
    //     if (_activeInstances.Count == 0) return;
    //     if (DuplicatePriorityResearch != null)
    //     {
    //         EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
    //         DuplicatePriorityResearch = null;
    //     }
    //     Debug.Log("Rebuilding " + _activeInstances.ElementAt(0).target.name);

    //     _duplicatePrioritySearchActive = true;
    //     DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutineOwnerless(DuplicatePrioritySearchCoroutine());
    // }

    // public static bool IsThereAnyPriorityConflict(string guid)
    // {
    //     if (!_actionsPrioritiesCount.ContainsKey(guid)) return false;

    //     foreach (var priorityCounts in _actionsPrioritiesCount[guid])
    //     {
    //         if (priorityCounts.Value > 1)
    //         {
    //             return true;
    //         }
    //     }

    //     return false;
    // }

    // public static int GetPriorityCountForAction(string guid, int priority)
    // {
    //     if (!_actionsPrioritiesCount.ContainsKey(guid)) return -1;
    //     if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return -1;

    //     return _actionsPrioritiesCount[guid][priority];
    // }

    // public static void AddPriority(string guid, int priority, string propertyPath, InputConfigSO inputConfig)
    // {
    //     if (!_actionsPriorities.ContainsKey(guid)) _actionsPriorities[guid] = new();
    //     if (!_actionsPrioritiesCount.ContainsKey(guid)) _actionsPrioritiesCount[guid] = new();

    //     if (!_actionsPriorities[guid].Contains(inputConfig)) _actionsPriorities[guid].Add(inputConfig);
    //     if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) _actionsPrioritiesCount[guid][priority] = 0;
    //     _actionsPrioritiesCount[guid][priority]++;
    // }

    // public static void RemovePriority(string guid, int priority, InputConfigSO inputConfig)
    // {
    //     if (!_actionsPriorities.ContainsKey(guid)) return;
    //     if (!_actionsPrioritiesCount.ContainsKey(guid)) return;
    //     if (!_actionsPriorities[guid].Contains(inputConfig)) return;
    //     _actionsPriorities[guid].Remove(inputConfig);
    //     if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return;
    //     _actionsPrioritiesCount[guid][priority]--;
    // }

    // public static void UpdatePriority(string guid, int oldPriority, int newPriority, string propertyPath, InputConfigSO inputConfig)
    // {
    //     if (oldPriority == newPriority) return;
    //     RemovePriority(guid, oldPriority, inputConfig);
    //     AddPriority(guid, newPriority, propertyPath, inputConfig);
    // }

    // private static IEnumerator DuplicatePrioritySearchCoroutine()
    // {
    //     // use temporary dictionaries so we don't leave a window where there is no data to read
    //     Dictionary<string, HashSet<InputConfigSO>> actionPrioritiesTemp = new();
    //     Dictionary<string, Dictionary<int, int>> actionPrioritiesCountTemp = new();

    //     var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>();

    //     // find all configs where action is found
    //     for (int i = allConfigs.Length - 1; i >= 0; i--)
    //     {
    //         // bool foundAction = false;
    //         InputConfigSO config = allConfigs[i];

    //         var inputAssetMapLists = config.GetInputAssetMaps();

    //         for (int j = 0; j < inputAssetMapLists.Count; j++)
    //         {
    //             var inputMapStructs = inputAssetMapLists[j].InputMapStructs;
    //             for (int k = 0; k < inputMapStructs.Count; k++)
    //             {
    //                 var InputActionEntries = inputMapStructs[k].InputActionEntries;
    //                 for (int l = 0; l < InputActionEntries.Count; l++)
    //                 {
    //                     InputActionStruct inputActionStruct = InputActionEntries[l];
    //                     string guid = inputActionStruct.Guid;
    //                     int priority = inputActionStruct.Priority;

    //                     // build priority dictionary
    //                     if (!actionPrioritiesCountTemp.ContainsKey(guid)) actionPrioritiesCountTemp[guid] = new();
    //                     var priorityCount = actionPrioritiesCountTemp[guid];
    //                     if (!priorityCount.ContainsKey(priority)) priorityCount[priority] = 0;
    //                     priorityCount[priority]++;

    //                     if (!actionPrioritiesTemp.ContainsKey(guid)) actionPrioritiesTemp[guid] = new();
    //                     var inputConfigs = actionPrioritiesTemp[guid];
    //                     inputConfigs.Add(config);
    //                 }
    //             }
    //         }
    //     }

    //     // when completed update the dictionaries with the fresh data
    //     _actionsPriorities = actionPrioritiesTemp;
    //     _actionsPrioritiesCount = actionPrioritiesCountTemp;

    //     List<InputConfigSOEditor> inputConfigSOEditors = _activeInstances.ToList();
    //     for (int i = inputConfigSOEditors.Count - 1; i >= 0; i--)
    //     {
    //         inputConfigSOEditors?[i].Repaint();
    //     }
    //     _duplicatePrioritySearchActive = false;
    //     yield break;
    // }

    // private PriorityAvailabilityEnum IsPriorityAvailable(string guid, int requestedPriority)
    // {
    //     if (_actionsPrioritiesCount.Count == 0) return PriorityAvailabilityEnum.NO_CONFLICT;
    //     if (!_actionsPrioritiesCount.ContainsKey(guid)) return PriorityAvailabilityEnum.NO_CONFLICT;

    //     int priorityCount = GetPriorityCountForAction(guid, requestedPriority);
    //     bool isThereAnyPriorityConflict = IsThereAnyPriorityConflict(guid);
    //     if (priorityCount == -1 || priorityCount - 1 > 0)
    //     {
    //         return PriorityAvailabilityEnum.SELF_CONFLICT;
    //     }
    //     else
    //     {
    //         if (isThereAnyPriorityConflict)
    //         {
    //             return PriorityAvailabilityEnum.SELF_AVAILABLE;
    //         }
    //         else
    //         {
    //             return PriorityAvailabilityEnum.NO_CONFLICT;
    //         }
    //     }
    // }

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
        SerializedProperty actionsList = mapProp.FindPropertyRelative("InputActionEntries");
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
                    ResetInputEntry(newAction, action);

                    // and update priority dictionaries
                    // AddPriority(actionGuid, 0, newAction.propertyPath, inputConfig);
                }

                serializedObject.ApplyModifiedProperties();
                InputConfigPriorityCache.RebuildPriorityDictionary();
            }
        }

        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {
            SerializedProperty InputActionEntries = mapProp.FindPropertyRelative("InputActionEntries");

            // for (int i = 0; i < InputActionEntries.arraySize; i++)
            // {
            //     SerializedProperty actionProp = InputActionEntries.GetArrayElementAtIndex(i);
            //     int priority = actionProp.FindPropertyRelative("Priority").intValue;
            //     RemovePriority(actionProp.FindPropertyRelative("Guid").stringValue, priority, inputConfig);
            // }

            _assetMapListProp.GetArrayElementAtIndex(inputAssetMapListIndex).FindPropertyRelative("InputMapStructs").DeleteArrayElementAtIndex(mapIndex);
            serializedObject.ApplyModifiedProperties();
            InputConfigPriorityCache.RebuildPriorityDictionary();
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

    private void ResetInputEntry(SerializedProperty actionProp, InputAction inputAction)
    {
        actionProp.FindPropertyRelative("Guid").stringValue = inputAction.id.ToString();
        actionProp.FindPropertyRelative("Enabled").boolValue = true;
        actionProp.FindPropertyRelative("Priority").intValue = 0;
        actionProp.FindPropertyRelative("NameOverride").stringValue = string.Empty;

        SerializedProperty promptSchemesProp = actionProp.FindPropertyRelative("PromptSchemes");
        promptSchemesProp.ClearArray();

        // CRITICAL FIX: Loop sequentially through ALL bindings to find composite headers.
        // If you filter out groups early via LINQ, Unity strips out the `isComposite` rows!
        var allBindings = inputAction.bindings;

        // Track unique control schemes manually across sequential tracking
        HashSet<string> processedSchemes = new HashSet<string>();

        int schemeIndexCounter = 0;

        for (int i = 0; i < allBindings.Count; i++)
        {
            var currentBinding = allBindings[i];

            // We only care about bindings that have assigned groups
            // If it's a composite header, it won't have a group, so we evaluate its child groups below
            string schemeName = currentBinding.groups;

            if (currentBinding.isComposite)
            {
                // Peek at the first child to inherit its control scheme group string
                if (i + 1 < allBindings.Count && !string.IsNullOrEmpty(allBindings[i + 1].groups))
                {
                    schemeName = allBindings[i + 1].groups;
                }
            }

            if (string.IsNullOrEmpty(schemeName)) continue;

            // Ensure we initialize the Prompt Scheme Serialized Array for this group if it's the first time seeing it
            if (!processedSchemes.Contains(schemeName))
            {
                processedSchemes.Add(schemeName);
                promptSchemesProp.InsertArrayElementAtIndex(schemeIndexCounter);

                var schemeProp = promptSchemesProp.GetArrayElementAtIndex(schemeIndexCounter);
                schemeProp.FindPropertyRelative("Scheme").stringValue = schemeName;

                // Clear the nested 'Prompts' array in case data was duplicated by Unity
                var initialPromptsClear = schemeProp.FindPropertyRelative("Prompts");
                if (initialPromptsClear != null) initialPromptsClear.arraySize = 0;

                schemeIndexCounter++;
            }

            // Fetch the corresponding SerializedProperty for our current active scheme row
            int targetSchemeIndex = GetSchemeIndex(promptSchemesProp, schemeName);
            if (targetSchemeIndex == -1) continue;

            var currentPromptSchemeProp = promptSchemesProp.GetArrayElementAtIndex(targetSchemeIndex);
            var promptsProp = currentPromptSchemeProp.FindPropertyRelative("Prompts");

            // --- CASE 1: Standalone Binding ---
            if (!currentBinding.isComposite && !currentBinding.isPartOfComposite)
            {
                string promptText = $"Press {InputActionEntry.BUTTON_PLACEHOLDER} to {inputAction.name}";
                AddPromptEntry(promptsProp, currentBinding.id.ToString(), promptText);
                continue;
            }

            // --- CASE 2: Composite Header Found ---
            if (currentBinding.isComposite)
            {
                string compositeTypePath = currentBinding.path;
                string promptText = "Press ";

                // Check if it's a modifier key profile (e.g. "ButtonWithOneModifier")
                if (compositeTypePath.Contains("Modifier", StringComparison.OrdinalIgnoreCase))
                {
                    // Gather modifiers sequentially
                    List<string> compositeParts = new List<string>();
                    int childIdx = i + 1;

                    while (childIdx < allBindings.Count && allBindings[childIdx].isPartOfComposite)
                    {
                        // Filter matching group sub-elements
                        if (allBindings[childIdx].groups == schemeName)
                        {
                            compositeParts.Add(InputActionEntry.BUTTON_PLACEHOLDER);
                        }
                        childIdx++;
                    }

                    promptText += string.Join(" + ", compositeParts) + $" to {inputAction.name}";
                }
                else
                {
                    // Layout composites like 2DVector (WASD) require single unified prompts 
                    promptText += $"{InputActionEntry.BUTTON_PLACEHOLDER} to {inputAction.name}";
                }

                AddPromptEntry(promptsProp, currentBinding.id.ToString(), promptText);

                // Skip loop processing past the composite items we just processed as a combined chunk
                while (i + 1 < allBindings.Count && allBindings[i + 1].isPartOfComposite)
                {
                    i++;
                }
            }
        }

        // Commit structural changes to storage
        actionProp.serializedObject.ApplyModifiedProperties();
    }

    private void AddPromptEntry(SerializedProperty promptsProp, string guid, string promptText)
    {
        if (promptsProp == null) return;

        int newIndex = promptsProp.arraySize;
        promptsProp.InsertArrayElementAtIndex(newIndex);

        var element = promptsProp.GetArrayElementAtIndex(newIndex);
        element.FindPropertyRelative("Guid").stringValue = guid;
        element.FindPropertyRelative("Prompt").stringValue = promptText;
    }

    private int GetSchemeIndex(SerializedProperty promptSchemesProp, string schemeName)
    {
        for (int i = 0; i < promptSchemesProp.arraySize; i++)
        {
            if (promptSchemesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Scheme").stringValue == schemeName)
            {
                return i;
            }
        }
        return -1;
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
        PriorityAvailabilityEnum priorityAvailable = InputConfigPriorityCache.IsPriorityAvailable(actionGUID, oldPriority);

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
            if (!InputConfigPriorityCache.ActionsPriorities.ContainsKey(actionGUID))
            {
                Debug.LogWarning("ActionGUID isn't inside the Priority Dictionary, click the button to update it");
            }
            else
            {
                Rect buttonRect = GUILayoutUtility.GetLastRect();
                Dictionary<InputConfigSO, string> actionPrioritiesPaths = new();
                foreach (var item in InputConfigPriorityCache.ActionsPriorities[actionGUID])
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
            // RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
            list.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            InputConfigPriorityCache.RebuildPriorityDictionary();
        }
        // if something changed check if priority did and in that case update
        else if (EditorGUI.EndChangeCheck())
        {
            if (oldPriority != newPriority)
            {
                _rebuildDeadline = EditorApplication.timeSinceStartup + _rebuildDelay;
            }
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
                    var newActionsList = newMap.FindPropertyRelative("InputActionEntries");
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
                    ResetInputEntry(newAction, action);

                    serializedObject.ApplyModifiedProperties();
                    InputConfigPriorityCache.RebuildPriorityDictionary();
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
                SerializedProperty InputActionEntries = inputMapStructs.GetArrayElementAtIndex(k).FindPropertyRelative("InputActionEntries");
                for (int l = 0; l < InputActionEntries.arraySize; l++)
                {
                    string guid = InputActionEntries.GetArrayElementAtIndex(l).FindPropertyRelative("Guid").stringValue;
                    if (guid == actionGuid)
                    {
                        return InputActionEntries.GetArrayElementAtIndex(l).propertyPath;
                    }
                }
            }
        }

        return null;
    }
}