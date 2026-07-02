using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

using PriorityAvailabilityEnum = InputConfigPriorityCache.PriorityAvailabilityEnum;
using UnityEditor.Rendering;

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

    private readonly string[] TOOLS_LIST = { "Priority", "Presentation" };
    private int _currentToolIndex;

    private void OnEnable()
    {
        EditorApplication.update += PriorityChangeCheck;
        InputConfigPriorityCache.OnRebuildCompleted += RefreshInspector;

        // Access the private [SerializeField] list from the target ScriptableObject
        _assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");

        // try load the last used InputActionAsset

        _alreadyCreatedArrayElementForAssetMapList = false;

        // checks whether there are maps not belonging to any InputActionAsset, this happens when the it's deleted
        InputActionAsset[] inputActionAssets = AssetDatabaseUtils.GetAssetsByType<InputActionAsset>();

        _mapsWithoutAsset = new();

        InputConfigSO inputConfigSO = target as InputConfigSO;
        _loaderAsset = inputConfigSO.LastUsedInputAsset;

        //TODO also implement this ondisable
        bool arrayChanged = false;

        for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            var assetType = inputAssetMapList.FindPropertyRelative("AssetType");
            if (assetType == null || assetType.objectReferenceValue == null)
            {
                _assetMapListProp.DeleteArrayElementAtIndex(i);
                arrayChanged = true;
                continue;
            }
            int mapsCount = inputAssetMapList.FindPropertyRelative("InputMapStructs").arraySize;
            if (mapsCount == 0)
            {
                _assetMapListProp.DeleteArrayElementAtIndex(i);
                arrayChanged = true;
            }
        }

        if (arrayChanged)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        InputMapStruct[] maps = inputConfigSO.GetInputAssetMaps().SelectMany(c => c.InputMapStructs).ToArray();

        _mapsWithoutAsset = new HashSet<string>(maps.Select(m => m.Guid));
        _actionsOrphan = new HashSet<string>(maps.SelectMany(m => m.InputActionEntries).Select(a => a.Guid));
        _bindingsOrphan = new HashSet<string>(maps.SelectMany(m => m.InputActionEntries).SelectMany(a => a.PromptSchemes).SelectMany(p => p.Prompts).Select(b => b.Guid));

        //TODO: populate the hashsets so when drawing an a element we know if it's valid or orphane and in that case there should be some visual feedback
        // like red or some, or for instance the button remove should be red or shi
        // also a remove all orphanes button a the top could be a thing
        foreach (var inputAsset in inputActionAssets)
        {
            foreach (var map in maps)
            {
                InputActionMap foundMap = inputAsset.FindActionMap(map.Guid);
                if (foundMap != null)
                {
                    _mapsWithoutAsset.Remove(map.Guid);
                }

                var actions = map.InputActionEntries.ToArray();
                foreach (var action in actions)
                {
                    InputAction foundAction;
                    if (foundMap == null)
                    {
                        foundAction = null;
                    }
                    else
                    {
                        foundAction = inputAsset.FindAction(action.Guid);
                    }

                    if (foundAction != null)
                    {
                        _actionsOrphan.Remove(action.Guid);
                    }

                    var bindings = action.PromptSchemes.SelectMany(c => c.Prompts).ToArray();
                    foreach (var binding in bindings)
                    {
                        if (foundAction == null)
                        {
                            continue;
                        }

                        if (foundAction.bindings.Any(b => b.id.ToString() == binding.Guid))
                        {
                            _bindingsOrphan.Remove(binding.Guid);
                        }
                    }
                }
            }
        }

        //Debug.Log($"orphans {_mapsWithoutAsset.Count} {_actionsOrphan.Count} {_bindingsOrphan.Count}");

        // loop through each map and check if it belongs to any inputActionAsset, if not add it to maps without an asset

    }

    void OnDisable()
    {
        EditorApplication.update -= PriorityChangeCheck;
        InputConfigPriorityCache.OnRebuildCompleted -= RefreshInspector;

        //TODO also implement this ondisable
        bool arrayChanged = false;

        for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
            var assetType = inputAssetMapList.FindPropertyRelative("AssetType");
            if (assetType == null || assetType.objectReferenceValue == null)
            {
                _assetMapListProp.DeleteArrayElementAtIndex(i);
                arrayChanged = true;
                continue;
            }
            int mapsCount = inputAssetMapList.FindPropertyRelative("InputMapStructs").arraySize;
            if (mapsCount == 0)
            {
                _assetMapListProp.DeleteArrayElementAtIndex(i);
                arrayChanged = true;
            }
        }

        if (arrayChanged)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
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
            // compares the 2 lists of actions, if they are the same it means they are poiting to the same InputActionAsset, meaning we've found the corresponding inputAssetMapList
            TypeVar typeVar = inputAssetMapList.FindPropertyRelative("AssetType").objectReferenceValue as TypeVar;
            if (typeVar != null)
            {
                var instance = (IInputActionCollection2)Activator.CreateInstance(typeVar.Type);

                HashSet<Guid> instanceActionsGuids = instance.Select(action => action.id).ToHashSet();
                HashSet<Guid> loaderAssetActionsGuids = _loaderAsset.Select(action => action.id).ToHashSet();

                bool areEquals = instanceActionsGuids.SetEquals(loaderAssetActionsGuids);

                if (areEquals)
                {
                    assetMapListIndex = inputAssetMapListIndex;
                    _alreadyCreatedArrayElementForAssetMapList = true;
                }
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
            _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
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
        else
        {
            if (!typeof(IInputActionCollection2).IsAssignableFrom(_loaderAssetInstanceType.Type))
            {
                Debug.LogWarning("Assigned TypeVar's Type doesn't belong to any C# files generated by an InputActionAsset");
                _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                return;
            }
            var instance = (IInputActionCollection2)Activator.CreateInstance(_loaderAssetInstanceType.Type);

            HashSet<Guid> instanceActionsGuids = instance.Select(action => action.id).ToHashSet();
            HashSet<Guid> loaderAssetActionsGuids = _loaderAsset.Select(action => action.id).ToHashSet();

            bool areEquals = instanceActionsGuids.SetEquals(loaderAssetActionsGuids);

            if (!areEquals)
            {
                Debug.LogWarning("Assign a TypeVar with Type being the C# generated script of the selected InputActionAsset to associate to the Reference Asset to edit overrides.");
                _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                return;
            }
        }

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

        _currentToolIndex = GUILayout.Toolbar(_currentToolIndex, TOOLS_LIST);

        // Draw Filtered Maps
        // Only render maps that exist in this InputConfigSO and that are present in the assigned loader asset
        // this is the filter step for ensuring we can only add/show maps that are present in this loader asset

        SerializedProperty inputMapStructs = _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("InputMapStructs");
        for (int i = 0; i < inputMapStructs.arraySize; i++)
        {
            DrawMapFoldout(inputMapStructs.GetArrayElementAtIndex(i), assetMapListIndex, i);
        }

        // LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        // {
        //     SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative(relativePropertyPath: "InputMapStructs");
        //     SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(mapIndex);
        //     if (assetMapGuids.Contains(guid))
        //     {
        //         DrawMapFoldout(mapElem, inputAssetMapListIndex, mapIndex);
        //     }
        // });

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
        var assetMap = _loaderAsset.actionMaps.FirstOrDefault(m => m.id.ToString() == guid);
        string mapName = mapProp.FindPropertyRelative("Name").stringValue;

        bool isOrphan = _mapsWithoutAsset.Contains(guid);

        GUI.enabled = assetMap != null;

        EditorGUILayout.BeginVertical("helpbox");

        // Header Row with Map Name and Remove Button
        EditorGUILayout.BeginHorizontal();

        Color guiColor = GUI.color;
        if (isOrphan) GUI.color = Color.softYellow;

        mapProp.isExpanded = EditorGUILayout.Foldout(mapProp.isExpanded, $"Map: {(isOrphan ? "(Deleted)" : "")}{mapName}", true);

        // Matches the field name in InputMapStruct
        SerializedProperty actionsList = mapProp.FindPropertyRelative("InputActionEntries");
        InputConfigSO inputConfig = target as InputConfigSO;

        if (assetMap != null)
        {
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
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Add all", "Adds all of the actions of the map"), GUILayout.Width(100)))
            {
            }
        }

        GUI.enabled = true;
        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {

            // for (int i = 0; i < InputActionEntries.arraySize; i++)
            // {
            //     SerializedProperty actionProp = InputActionEntries.GetArrayElementAtIndex(i);
            //     int priority = actionProp.FindPropertyRelative("Priority").intValue;
            //     RemovePriority(actionProp.FindPropertyRelative("Guid").stringValue, priority, inputConfig);
            // }

            mapProp.isExpanded = false;
            _assetMapListProp.GetArrayElementAtIndex(inputAssetMapListIndex).FindPropertyRelative("InputMapStructs").DeleteArrayElementAtIndex(mapIndex);
            serializedObject.ApplyModifiedProperties();
            InputConfigPriorityCache.RebuildPriorityDictionary();
            GUIUtility.ExitGUI();
            return;
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = assetMap != null;

        // Actions Section
        if (mapProp.isExpanded)
        {
            EditorGUI.indentLevel++;

            switch (_currentToolIndex)
            {
                // Priority
                case 0:
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
                        DrawActionRowPriority(actionElem, assetMap, actionsList, j);
                    }
                    break;
                // Presentation
                case 1:
                    // Column Header Labels
                    EditorGUILayout.BeginHorizontal();
                    // EditorGUILayout.LabelField("View all", EditorStyles.miniBoldLabel, GUILayout.Width(100));
                    EditorGUILayout.LabelField("Action Name", EditorStyles.miniBoldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField("Name Override", EditorStyles.miniBoldLabel, GUILayout.MinWidth(100));
                    // EditorGUILayout.LabelField("Priority", EditorStyles.miniBoldLabel, GUILayout.MinWidth(60), GUILayout.MaxWidth(100));
                    GUILayout.Space(30);
                    EditorGUILayout.EndHorizontal();

                    for (int j = 0; j < actionsList.arraySize; j++)
                    {
                        SerializedProperty actionElem = actionsList.GetArrayElementAtIndex(j);
                        DrawActionRowPresentation(actionElem, assetMap, actionsList, j);
                    }
                    break;
            }

            // Add Action Button at the end of the list
            DrawAddActionMenu(actionsList, assetMap);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        GUI.color = guiColor;
        GUI.enabled = true;
    }

    private void ResetInputEntry(SerializedProperty actionProp, InputAction inputAction)
    {
        actionProp.FindPropertyRelative("Guid").stringValue = inputAction.id.ToString();
        actionProp.FindPropertyRelative("Name").stringValue = inputAction.name;
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
                schemeProp.FindPropertyRelative("Scheme").stringValue = schemeName.Trim(';');

                // Clear the nested 'Prompts' array in case data was duplicated by Unity
                var initialPromptsClear = schemeProp.FindPropertyRelative("Prompts");
                if (initialPromptsClear != null) initialPromptsClear.arraySize = 0;

                schemeIndexCounter++;
            }

            // Fetch the corresponding SerializedProperty for our current active scheme row
            int targetSchemeIndex = GetSchemeIndex(promptSchemesProp, schemeName.Trim(';'));
            if (targetSchemeIndex == -1) continue;

            var currentPromptSchemeProp = promptSchemesProp.GetArrayElementAtIndex(targetSchemeIndex);
            var promptsProp = currentPromptSchemeProp.FindPropertyRelative("Prompts");

            // --- CASE 1: Standalone Binding ---
            if (!currentBinding.isComposite && !currentBinding.isPartOfComposite)
            {
                string promptText = $"{InputActionEntry.BUTTON_PLACEHOLDER} {inputAction.name}";
                AddPromptEntry(promptsProp, currentBinding.id.ToString(), currentBinding.ToDisplayString(), promptText);
                continue;
            }

            // --- CASE 2: Composite Header Found ---
            if (currentBinding.isComposite)
            {
                string compositeTypePath = currentBinding.path;
                string promptText = string.Empty;

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

                    promptText += string.Join(" + ", compositeParts) + $" {inputAction.name}";
                }
                else
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

                    promptText += string.Join("/", compositeParts) + $" {inputAction.name}";
                    // Layout composites like 2DVector (WASD) require single unified prompts 
                    // promptText += $"{InputActionEntry.BUTTON_PLACEHOLDER} {inputAction.name}";
                }

                AddPromptEntry(promptsProp, currentBinding.id.ToString(), inputAction.GetBindingDisplayString(i), promptText);

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

    private void AddPromptEntry(SerializedProperty promptsProp, string guid, string name, string promptText)
    {
        if (promptsProp == null) return;

        int newIndex = promptsProp.arraySize;
        promptsProp.InsertArrayElementAtIndex(newIndex);

        var element = promptsProp.GetArrayElementAtIndex(newIndex);
        element.FindPropertyRelative("Guid").stringValue = guid;
        element.FindPropertyRelative("Name").stringValue = name;
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
    private void DrawActionRowPriority(SerializedProperty actionProp, InputActionMap assetMap, SerializedProperty list, int index)
    {
        string actionGUID = actionProp.FindPropertyRelative("Guid").stringValue;
        var action = assetMap?.actions.FirstOrDefault(a => a.id.ToString() == actionGUID);
        string actionName = actionProp.FindPropertyRelative("Name").stringValue;

        int oldPriority = actionProp.FindPropertyRelative("Priority").intValue;

        Color guiColor = GUI.color;
        GUI.enabled = action != null;
        // draw the action name instead of the guid, then the enabled toggle, the priority field and a remove button
        EditorGUILayout.BeginHorizontal();

        // view all button
        string viewAllButtonText = string.Empty;
        string viewAllButtonTooltip = string.Empty;
        PriorityAvailabilityEnum priorityAvailable = InputConfigPriorityCache.IsPriorityAvailable(actionGUID, oldPriority);

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
                PopupWindow.Show(buttonRect, new PopupPriorityHelper(actionPrioritiesPaths, actionName ?? "Unknown"));
            }
        }

        GUI.color = guiColor;

        bool isOrphan = false;
        if (_actionsOrphan.Contains(actionGUID))
        {
            isOrphan = true;
            GUI.color = Color.orange;
        }

        // action name

        actionName = isOrphan ? $"(Deleted) {actionName}" : actionName ?? "Unknown";

        EditorGUILayout.LabelField(actionName, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
        // enabled status

        // check if anything changes
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));
        // priority
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.MinWidth(60));

        int newPriority = actionProp.FindPropertyRelative("Priority").intValue;

        GUI.enabled = true;
        // remove button
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            // RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
            list.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            InputConfigPriorityCache.RebuildPriorityDictionary();
            GUIUtility.ExitGUI();
        }
        // if something changed check if priority did and in that case update
        else if (EditorGUI.EndChangeCheck())
        {
            if (oldPriority != newPriority)
            {
                _rebuildDeadline = EditorApplication.timeSinceStartup + _rebuildDelay;
            }
        }
        GUI.enabled = action != null;

        EditorGUILayout.EndHorizontal();

        GUI.color = guiColor;
        GUI.enabled = true;
    }

    private void DrawActionRowPresentation(SerializedProperty actionProp, InputActionMap assetMap, SerializedProperty list, int index)
    {
        string actionGUID = actionProp.FindPropertyRelative("Guid").stringValue;
        var action = assetMap?.actions.FirstOrDefault(a => a.id.ToString() == actionGUID);
        var bindings = action?.bindings.Select(b => b.id.ToString()).ToHashSet();
        string actionName = actionProp.FindPropertyRelative("Name").stringValue;

        int oldPriority = actionProp.FindPropertyRelative("Priority").intValue;

        Color guiColor = GUI.color;
        GUI.enabled = action != null;
        // draw the action name instead of the guid, then the enabled toggle, the priority field and a remove button

        bool isOrphan = false;
        if (_actionsOrphan.Contains(actionGUID))
        {
            isOrphan = true;
            GUI.color = Color.orange;
        }

        //TODO: update remove orphans, so it removes all, not just maps 
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.BeginHorizontal();
        Rect foldoutRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(150));
        Rect indentedFoldoutRect = EditorGUI.IndentedRect(foldoutRect);
        string cleanLabel = $"{((action == null) ? "(Deleted)" : string.Empty)} {actionName}";

        // 2. Use EditorGUI instead of EditorGUILayout to draw it exactly in that bounding box
        actionProp.isExpanded = EditorGUI.Foldout(indentedFoldoutRect, actionProp.isExpanded, cleanLabel, true);
        actionProp.FindPropertyRelative("NameOverride").stringValue = EditorGUILayout.TextField(actionProp.FindPropertyRelative("NameOverride").stringValue);
        if (action != null && GUILayout.Button(new GUIContent("+", "Adds all bindings from the remaining"), GUILayout.Width(25)))
        {
            GenericMenu genericMenu = new();

            SerializedProperty promptSchemesProp = actionProp.FindPropertyRelative("PromptSchemes");

            promptSchemesProp.ClearArray();

            // CRITICAL FIX: Loop sequentially through ALL bindings to find composite headers.
            // If you filter out groups early via LINQ, Unity strips out the `isComposite` rows!
            var allBindings = action.bindings;

            HashSet<string> alreadyAddedBindings = new();
            for (int i = 0; i < promptSchemesProp.arraySize; i++)
            {
                SerializedProperty bindingsPrompts = promptSchemesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Prompts");
                // string schemeName = promptSchemesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Scheme").stringValue;

                for (int j = 0; j < bindingsPrompts.arraySize; j++)
                {
                    SerializedProperty binding = bindingsPrompts.GetArrayElementAtIndex(j);
                    string guid = binding.FindPropertyRelative("Guid").stringValue;
                    alreadyAddedBindings.Add(guid);
                }
            }

            int schemeIndexCounter = 0;

            // Track unique control schemes manually across sequential tracking
            HashSet<string> processedSchemes = new HashSet<string>();
            for (int i = 0; i < allBindings.Count; i++)
            {
                var currentBinding = allBindings[i];

                if (currentBinding.isPartOfComposite || alreadyAddedBindings.Contains(currentBinding.id.ToString()))
                {
                    continue;
                }
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
                    schemeProp.FindPropertyRelative("Scheme").stringValue = schemeName.Trim(';');

                    // Clear the nested 'Prompts' array in case data was duplicated by Unity
                    var initialPromptsClear = schemeProp.FindPropertyRelative("Prompts");
                    if (initialPromptsClear != null) initialPromptsClear.arraySize = 0;

                    schemeIndexCounter++;
                }

                // Fetch the corresponding SerializedProperty for our current active scheme row
                int targetSchemeIndex = GetSchemeIndex(promptSchemesProp, schemeName.Trim(';'));
                if (targetSchemeIndex == -1) continue;

                var currentPromptSchemeProp = promptSchemesProp.GetArrayElementAtIndex(targetSchemeIndex);
                var promptsProp = currentPromptSchemeProp.FindPropertyRelative("Prompts");

                // --- CASE 1: Standalone Binding ---
                if (!currentBinding.isComposite && !currentBinding.isPartOfComposite)
                {
                    string promptText = $"{InputActionEntry.BUTTON_PLACEHOLDER} {actionName}";
                    AddPromptEntry(promptsProp, currentBinding.id.ToString(), currentBinding.ToDisplayString(), promptText);
                    continue;
                }

                // --- CASE 2: Composite Header Found ---
                if (currentBinding.isComposite)
                {
                    string compositeTypePath = currentBinding.path;
                    string promptText = string.Empty;

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

                        promptText += string.Join(" + ", compositeParts) + $" {actionName}";
                    }
                    else
                    {
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

                        // Layout composites like 2DVector (WASD) require single unified prompts 
                        promptText += string.Join("/", compositeParts) + $" {actionName}";
                    }

                    AddPromptEntry(promptsProp, currentBinding.id.ToString(), action.GetBindingDisplayString(i), promptText);

                    // Skip loop processing past the composite items we just processed as a combined chunk

                    while (i + 1 < allBindings.Count && allBindings[i + 1].isPartOfComposite)
                    {
                        i++;
                    }
                }
            }

        }

        GUI.enabled = true;
        // remove button
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            // RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
            actionProp.isExpanded = false;
            list.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            InputConfigPriorityCache.RebuildPriorityDictionary();

            GUIUtility.ExitGUI();
        }
        GUI.enabled = action != null;
        EditorGUILayout.EndHorizontal();


        if (actionProp != null && actionProp.isExpanded)
        {
            EditorGUILayout.Space(3);
            SerializedProperty promptSchemesProp = actionProp.FindPropertyRelative("PromptSchemes");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.indentLevel++;
                // 2. Add manual horizontal spacing matching Unity's standard indent size (15 pixels per level)
                GUILayout.Space(EditorGUI.indentLevel * 15f);

                // 3. Put your VerticalScope inside it. The entire box will now shift right!
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUI.indentLevel--;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel, GUILayout.Width(170));
                        EditorGUILayout.LabelField("Prompt Override", EditorStyles.boldLabel);
                    }

                    for (int i = 0; i < promptSchemesProp.arraySize; i++)
                    {
                        SerializedProperty bindingsPrompts = promptSchemesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Prompts");
                        string schemeName = promptSchemesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Scheme").stringValue;

                        for (int j = 0; j < bindingsPrompts.arraySize; j++)
                        {
                            SerializedProperty binding = bindingsPrompts.GetArrayElementAtIndex(j);
                            string bindingName = binding.FindPropertyRelative("Name").stringValue;
                            string guid = binding.FindPropertyRelative("Guid").stringValue;

                            // binding doesn't exist in the action, disable the UI
                            if (action != null)
                                GUI.enabled = bindings.Contains(guid);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (_bindingsOrphan.Contains(guid))
                                {
                                    GUI.color = Color.orange;
                                }

                                EditorGUILayout.LabelField($"{(_bindingsOrphan.Contains(guid) ? "(Deleted) " : string.Empty)}" + bindingName + $" ({schemeName})", GUILayout.Width(170));
                                binding.FindPropertyRelative("Prompt").stringValue = EditorGUILayout.TextField(binding.FindPropertyRelative("Prompt").stringValue);

                                // if action exists but binding doesn't, provide a way to remove it
                                if (action != null)
                                {
                                    // if (!bindings.Contains(guid))
                                    {
                                        GUI.enabled = true;
                                        // remove button
                                        if (GUILayout.Button("X", GUILayout.Width(25)))
                                        {
                                            // RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
                                            bindingsPrompts.DeleteArrayElementAtIndex(j);
                                            serializedObject.ApplyModifiedProperties();
                                            InputConfigPriorityCache.RebuildPriorityDictionary();
                                            GUIUtility.ExitGUI();
                                        }
                                        GUI.enabled = bindings.Contains(guid);
                                    }
                                }
                            }

                            GUI.color = guiColor;
                            GUI.enabled = true;
                        }
                    }
                    EditorGUI.indentLevel++;
                }
                EditorGUI.indentLevel--;
            }
        }
        EditorGUILayout.EndVertical();
        // view all button
        // string viewAllButtonText = string.Empty;
        // string viewAllButtonTooltip = string.Empty;
        // PriorityAvailabilityEnum priorityAvailable = InputConfigPriorityCache.IsPriorityAvailable(actionGUID, oldPriority);

        // switch (priorityAvailable)
        // {
        //     case PriorityAvailabilityEnum.SELF_AVAILABLE:
        //         viewAllButtonText = "View All";
        //         viewAllButtonTooltip = "This priority doesn't conflict with other configs, but there are conflicts to resolve";
        //         GUI.color = Color.Lerp(guiColor, Color.yellow, 0.4f);
        //         break;
        //     case PriorityAvailabilityEnum.SELF_CONFLICT:
        //         viewAllButtonText = "Fix Priority";
        //         viewAllButtonTooltip = "This action's priority conflicts with other configs";
        //         GUI.color = Color.Lerp(Color.red, Color.yellow, 0.8f);
        //         break;
        //     case PriorityAvailabilityEnum.NO_CONFLICT:
        //         viewAllButtonText = "View All";
        //         viewAllButtonTooltip = "No conflicts for this action";
        //         GUI.color = Color.paleGreen;
        //         break;
        // }

        // if (GUILayout.Button(new GUIContent(viewAllButtonText, viewAllButtonTooltip), GUILayout.Width(100)))
        // {
        //     if (!InputConfigPriorityCache.ActionsPriorities.ContainsKey(actionGUID))
        //     {
        //         Debug.LogWarning("ActionGUID isn't inside the Priority Dictionary, click the button to update it");
        //     }
        //     else
        //     {
        //         Rect buttonRect = GUILayoutUtility.GetLastRect();
        //         Dictionary<InputConfigSO, string> actionPrioritiesPaths = new();
        //         foreach (var item in InputConfigPriorityCache.ActionsPriorities[actionGUID])
        //         {
        //             actionPrioritiesPaths[item] = GetPriorityPropertyPath(actionGUID, item);
        //         }
        //         PopupWindow.Show(buttonRect, new PopupPriorityHelper(actionPrioritiesPaths, actionName ?? "Unknown"));
        //     }
        // }

        GUI.color = guiColor;


        // // action name

        // actionName = isOrphan ? $"(Deleted) {actionName}" : actionName ?? "Unknown";

        // EditorGUILayout.LabelField(actionName, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
        // // enabled status

        // // check if anything changes
        // EditorGUI.BeginChangeCheck();
        // EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));
        // // priority
        // EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.MinWidth(60), GUILayout.MaxWidth(100));

        // int newPriority = actionProp.FindPropertyRelative("Priority").intValue;
        // // remove button
        // if (GUILayout.Button("X", GUILayout.Width(25)))
        // {
        //     // RemovePriority(actionGUID, newPriority, (InputConfigSO)target);
        //     list.DeleteArrayElementAtIndex(index);
        //     serializedObject.ApplyModifiedProperties();
        //     InputConfigPriorityCache.RebuildPriorityDictionary();
        // }
        // // if something changed check if priority did and in that case update
        // else if (EditorGUI.EndChangeCheck())
        // {
        //     if (oldPriority != newPriority)
        //     {
        //         _rebuildDeadline = EditorApplication.timeSinceStartup + _rebuildDelay;
        //     }
        // }

        GUI.color = guiColor;
        GUI.enabled = true;
        EditorGUILayout.Space(1);
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
                    newMap.FindPropertyRelative("Name").stringValue = map.name;

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
        GUI.enabled = assetMap != null;
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

        GUI.enabled = true;
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