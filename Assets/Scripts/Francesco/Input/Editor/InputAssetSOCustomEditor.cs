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
    private bool _duplicatePrioritySearchActive;

    private void OnEnable()
    {
        // Access the private [SerializeField] list from the target ScriptableObject
        _assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");

        // try load the last used InputActionAsset
        _loaderAsset = serializedObject.FindProperty("_lastUsedInputAsset").objectReferenceValue as InputActionAsset;

        _alreadyCreatedArrayElementForAssetMapList = false;
        DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);

        InputActionAsset[] inputActionAssets = AssetDatabaseUtils.GetAssetsByType<InputActionAsset>();

        _mapsWithoutGUID = new();

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
            if (!found) _mapsWithoutGUID.Add(guid);
        });

    }

    void OnDisable()
    {
        Debug.Log("DISABLE");
        // remove temporary and unused InputAssetMapList
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

        if (DuplicatePriorityResearch != null)
        {
            EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
        }

    }


    /// <summary>
    /// Updates the serialized object and draws the custom inspector elements, 
    /// including the loader field and the filtered map list.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = !_duplicatePrioritySearchActive;

        // 1. Loader Field - Used as a temporary lens to resolve GUIDs to Names
        EditorGUILayout.LabelField("Editor Workspace", EditorStyles.boldLabel);
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

            serializedObject.FindProperty("_lastUsedInputAsset").objectReferenceValue = _loaderAsset;

            serializedObject.ApplyModifiedProperties();
            _alreadyCreatedArrayElementForAssetMapList = false;
        }

        if (_loaderAsset == null)
        {
            EditorGUILayout.LabelField($"There are currently: {GetTotalMaps()} maps");
            EditorGUILayout.Space();
            DrawWarningForMapsWithoutAnAsset();
            EditorGUILayout.HelpBox("Assign an Input Asset to edit overrides.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            return;
        }

        // look for the InputAssetMapList index corresponding to the loader asset

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

        // at this point we have the mapListAsset instance
        // we make 
        _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = (TypeVar)EditorGUILayout.ObjectField("TypeVar input c# script", _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue, typeof(TypeVar), false);
        _loaderAssetInstanceType = (TypeVar)_assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue;

        // AssetType changed, we need to update 
        // if (previousTypeVar != _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue)
        // {
        //     LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        //     {
        //         if (assetMapGuids.Contains(guid))
        //         {
        //             inputAssetMapList.FindPropertyRelative("AssetType").objectReferenceValue = p
        //         }
        //     });
        // }

        if (_assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Assign a TypeVar to associate to the Reference Asset to edit overrides.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();

        DrawWarningForMapsWithoutAnAsset();

        EditorGUILayout.Space();

        // 2. Add Map Button ➕
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

        // 3. Draw Filtered Maps
        // Only render maps that exist in this InputConfigSO and that are present in the assigned loader asset
        // this is the filter step for ensuring we can only add/show maps that are present in this loader asset

        // get all of the guids of the maps in the current loader asset

        LoopThroughMaps((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
        {
            SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative(relativePropertyPath: "InputMapStructs");
            SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(mapIndex);
            if (assetMapGuids.Contains(guid))
            {
                DrawMapFoldout(mapElem, inputAssetMapListIndex, mapIndex);
            }
        });


        // // cycle through the list of maps of this SO and only draw the ones that are present in the assigned loader asset
        // for (int i = 0; i < _assetMapListProp.arraySize; i++)
        // {
        //     SerializedProperty mapElem = _assetMapListProp.GetArrayElementAtIndex(i);
        //     string guid = mapElem.FindPropertyRelative("Guid").stringValue;

        //     if (assetMapGuids.Contains(guid))
        //     {
        //         DrawMapFoldout(mapElem, i);
        //     }
        // }

        serializedObject.ApplyModifiedProperties();
    }

    private Dictionary<string, Dictionary<SerializedProperty, InputConfigSO>> _actionsPriorities = new();
    private Dictionary<string, Dictionary<int, int>> _actionsPrioritiesCount = new();

    private int GetTotalPriorityCountForAction(string guid)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return -1;

        int count = 0;

        foreach (var priorityCounts in _actionsPrioritiesCount[guid])
        {
            count += priorityCounts.Value;
        }

        return count;
    }

    private int GetPriorityCountForAction(string guid, int priority)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return -1;
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return -1;

        return _actionsPrioritiesCount[guid][priority];
    }

    private void AddPriorityFromCount(string guid, int priority)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return;
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) _actionsPrioritiesCount[guid][priority] = 0;
        _actionsPrioritiesCount[guid][priority]++;
    }

    private void RemovePriorityFromCount(string guid, int priority)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return;
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return;
        _actionsPrioritiesCount[guid][priority]--;
    }

    private IEnumerator DuplicatePrioritySearchCoroutine()
    {
        _duplicatePrioritySearchActive = true;
        _actionsPriorities.Clear();
        _actionsPrioritiesCount.Clear();

        var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>();
        //    .Where(config => config.GetInputAssetMaps(_loaderAssetInstanceType.Type).Count > 0).ToList();
        // allConfigs.Remove((InputConfigSO)target);

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
                if (typeVar.Type == null) continue;

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
                        var serializedActions = _actionsPriorities[guid];
                        serializedActions[inputActionStructs.GetArrayElementAtIndex(l)] = config;
                    }
                }
            }

            // var mapLists = config.GetInputAssetMaps(_loaderAssetInstanceType.Type);
            // foreach (var mapList in mapLists)
            // {
            //     foreach (var mapStruct in mapList.InputMapStructs)
            //     {
            //         var actionStruct = mapStruct.GetInputActionStruct(actionGUID);
            //         if (actionStruct.HasValue)
            //         {
            //             unavailablePriorities.Add(actionStruct.Value.Priority);
            //             foundAction = true;
            //             break;
            //         }
            //     }
            //     if (foundAction) break;
            // }
            // // if action wasn't found in this config we remove it
            // if (!foundAction)
            // {
            //     allConfigs.RemoveAt(i);
            // }
        }

        // if (unavailablePriorities.Contains(actionProp.FindPropertyRelative("Priority").intValue))
        // {
        //     int priorityValue = 0;

        //     while (unavailablePriorities.Contains(priorityValue))
        //     {
        //         priorityValue++;
        //     }

        //     actionProp.FindPropertyRelative("Priority").intValue = priorityValue;

        //     int actionPriority = actionProp.FindPropertyRelative("Priority").intValue;
        // }

        _duplicatePrioritySearchActive = false;
        yield break;
    }

    //TODO: could switch to just saving for each action the configSO and the propertyPath as a string, then get back the property from that
    // by getting the serializedOBject from the SO and then using the path to get the fresh SerializedProp
    private bool IsPriorityAvailable(SerializedProperty actionProperty, string guid, int requestedPriority)
    {
        if (_actionsPrioritiesCount.Count == 0) return false;
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return false;

        //TODO: if value -1 it means there's no valid entry in the dictionary, so i should just create it
        int priorityCount = GetPriorityCountForAction(guid, requestedPriority);
        if (priorityCount == -1 || priorityCount > 0) return false;

        if (_duplicatePrioritySearchActive) return false;
        // can
        if (_actionsPriorities.Count == 0)
        {
            Debug.LogWarning("Priority Dictionary is empty");
            return false;
        }
        if (!_actionsPriorities.ContainsKey(guid))
        {
            Debug.LogWarning("No such actionGUID inside Priority Dictionary");
            return false;
        }

        bool priorityConflict = false;
        foreach (var actionProp in _actionsPriorities[guid].Keys)
        {
            if (ReferenceEquals(
        actionProp.serializedObject.targetObject,
        actionProperty.serializedObject.targetObject)
    && actionProp.propertyPath == actionProperty.propertyPath)
                continue;
            int priority = actionProp.FindPropertyRelative("Priority").intValue;
            if (requestedPriority == priority)
            {
                priorityConflict = true;
                break;
            }
        }

        return !priorityConflict;
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

    private HashSet<string> _mapsWithoutGUID = new();
    private void DrawWarningForMapsWithoutAnAsset()
    {

        // for (int i = 0; i < _assetMapListProp.arraySize; i++)
        // {
        //     SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
        //     SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");

        //     for (int j = 0; j < mapStructList.arraySize; j++)
        //     {
        //         SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(i);
        //         string guid = mapElem.FindPropertyRelative("Guid").stringValue;

        //         bool found = false;
        //         foreach (var item in inputActionAssets)
        //         {
        //             if (item.FindActionMap(nameOrId: guid) != null)
        //             {
        //                 found = true;
        //                 break;
        //             }
        //         }
        //         if (!found) mapsWithoutGUID.Add(guid);
        //     }
        // }

        if (_mapsWithoutGUID.Count == 0) return;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.HelpBox($"There are {_mapsWithoutGUID.Count} maps with missing InputActionAsset", MessageType.Warning);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                LoopThroughMapsReverse((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
                {
                    SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");
                    if (_mapsWithoutGUID.Contains(guid))
                    {
                        mapStructList.DeleteArrayElementAtIndex(mapIndex);
                        _mapsWithoutGUID.Remove(guid);
                    }
                });
                // for (int i = _assetMapListProp.arraySize - 1; i >= 0; i--)
                // {
                //     SerializedProperty inputAssetMapList = _assetMapListProp.GetArrayElementAtIndex(i);
                //     SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");

                //     for (int j = mapStructList.arraySize - 1; j >= 0; j--)
                //     {
                //         SerializedProperty mapElem = mapStructList.GetArrayElementAtIndex(i);
                //         string guid = mapElem.FindPropertyRelative("Guid").stringValue;
                //         if (mapsWithoutGUID.Contains(guid))
                //         {
                //             mapStructList.DeleteArrayElementAtIndex(i);
                //             mapsWithoutGUID.Remove(guid);
                //         }
                //     }
                // }
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action">action guid, inputAssetMapList, inputAssetMapListIndex, mapIndex</param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action">action guid, inputAssetMapList, inputAssetMapListIndex, mapIndex</param>
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

                    int index = actionsList.arraySize;
                    actionsList.InsertArrayElementAtIndex(index);
                    var newAction = actionsList.GetArrayElementAtIndex(index);
                    newAction.FindPropertyRelative("Guid").stringValue = actionGuid;
                    newAction.FindPropertyRelative("Enabled").boolValue = true;
                    newAction.FindPropertyRelative("Priority").intValue = 0;

                    AddPriorityFromCount(actionGuid, 0);

                    serializedObject.ApplyModifiedProperties();
                }
                if (DuplicatePriorityResearch != null)
                {
                    EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
                    DuplicatePriorityResearch = null;
                }
                DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);
            }
        }

        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {
            SerializedProperty inputActionStructs = mapProp.FindPropertyRelative("InputActionStructs");

            for (int i = 0; i < inputActionStructs.arraySize; i++)
            {
                int priority = inputActionStructs.GetArrayElementAtIndex(i).FindPropertyRelative("Priority").intValue;
                RemovePriorityFromCount(inputActionStructs.GetArrayElementAtIndex(i).FindPropertyRelative("Guid").stringValue, priority);
            }

            _assetMapListProp.GetArrayElementAtIndex(inputAssetMapListIndex).FindPropertyRelative("InputMapStructs").DeleteArrayElementAtIndex(mapIndex);
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
            EditorGUILayout.LabelField("Action Name", EditorStyles.miniBoldLabel, GUILayout.MinWidth(100));
            EditorGUILayout.LabelField("On", EditorStyles.miniBoldLabel, GUILayout.Width(40));
            EditorGUILayout.LabelField("Priority", EditorStyles.miniBoldLabel, GUILayout.Width(60));
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

        // draw the action name instead of the guid, then the enabled toggle and the priority field
        EditorGUILayout.BeginHorizontal();
        bool isActionInPriorityDictionary = _actionsPriorities.ContainsKey(actionGUID);
        string viewAllButtonText = "View All";
        bool priorityAvailable = IsPriorityAvailable(actionProp, actionGUID, actionProp.FindPropertyRelative("Priority").intValue);
        if (isActionInPriorityDictionary)
        {
            if (!priorityAvailable)
            {
                viewAllButtonText = "Fix Priority";
            }
            else if (_actionsPriorities[actionGUID].ContainsKey(actionProp))
            {

            }
        }
        else
        {
            viewAllButtonText = "Key not found";
        }

        Color guiColor = GUI.color;

        if (!priorityAvailable || !isActionInPriorityDictionary)
        {
            GUI.color = Color.Lerp(Color.red, Color.yellow, 0.8f);
        }

        if (GUILayout.Button(viewAllButtonText, GUILayout.Width(100)))
        {
            if (!_actionsPriorities.ContainsKey(actionGUID))
            {
                Debug.LogWarning("ActionGUID isn't inside the Priority Dictionary, click the button to update it");
            }
            else
            {
                Rect buttonRect = GUILayoutUtility.GetLastRect();
                // buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
                PopupWindow.Show(buttonRect, new PopupPriorityHelper(_actionsPriorities[actionGUID], action?.name ?? "Unknown"));
            }
        }

        GUI.color = guiColor;
        // if (!IsPriorityAvailable(actionProp, actionGUID, actionProp.FindPropertyRelative("Priority").intValue))
        // {
        //     EditorGUILayout.BeginVertical(GUILayout.Height(EditorGUIUtility.singleLineHeight));
        //     EditorGUILayout.HelpBox("", MessageType.Warning);
        //     EditorGUILayout.EndVertical();
        // }
        EditorGUILayout.LabelField(action?.name ?? "Unknown", GUILayout.MinWidth(100));
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));

        using (new EditorGUILayout.HorizontalScope())
        {

            EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.Width(60));
        }

        //var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>()
        //    .Where(config => config.GetInputAssetMaps(_loaderAssetInstanceType.Type).Count > 0).ToList();
        //allConfigs.Remove((InputConfigSO)target);

        //HashSet<int> unavailablePriorities = new();
        //// find all configs where action is found
        //for (int i = allConfigs.Count - 1; i >= 0; i--)
        //{
        //    bool foundAction = false;
        //    InputConfigSO config = allConfigs.ElementAt(i);
        //    var mapLists = config.GetInputAssetMaps(_loaderAssetInstanceType.Type);
        //    foreach (var mapList in mapLists)
        //    {
        //        foreach (var mapStruct in mapList.InputMapStructs)
        //        {
        //            var actionStruct = mapStruct.GetInputActionStruct(actionGUID);
        //            if (actionStruct.HasValue)
        //            {
        //                unavailablePriorities.Add(actionStruct.Value.Priority);
        //                foundAction = true;
        //                break;
        //            }
        //        }
        //        if (foundAction) break;
        //    }
        //    // if action wasn't found in this config we remove it
        //    if (!foundAction)
        //    {
        //        allConfigs.RemoveAt(i);
        //    }
        //}

        //if (unavailablePriorities.Contains(actionProp.FindPropertyRelative("Priority").intValue))
        //{
        //    int priorityValue = 0;

        //    while (unavailablePriorities.Contains(priorityValue))
        //    {
        //        priorityValue++;
        //    }

        //    actionProp.FindPropertyRelative("Priority").intValue = priorityValue;

        //    int actionPriority = actionProp.FindPropertyRelative("Priority").intValue;
        //}

        // remove button
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            list.DeleteArrayElementAtIndex(index);
            RemovePriorityFromCount(actionGUID, actionProp.FindPropertyRelative("Priority").intValue);
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
                    //var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>()
                    //    .Where(config => config.GetInputAssetMaps(_loaderAssetInstanceType.Type).Count > 0).ToList();
                    //allConfigs.Remove((InputConfigSO)target);
                    //HashSet<int> unavailablePriorities = new();
                    //// find all configs where action is found
                    //for (int i = allConfigs.Count - 1; i >= 0; i--)
                    //{
                    //    bool foundAction = false;
                    //    InputConfigSO config = allConfigs.ElementAt(i);
                    //    var mapLists = config.GetInputAssetMaps(_loaderAssetInstanceType.Type);
                    //    foreach (var mapList in mapLists)
                    //    {
                    //        foreach (var mapStruct in mapList.InputMapStructs)
                    //        {
                    //            var actionStruct = mapStruct.GetInputActionStruct(actionGuid);
                    //            if (actionStruct.HasValue)
                    //            {
                    //                unavailablePriorities.Add(actionStruct.Value.Priority);
                    //                foundAction = true;
                    //                break;
                    //            }
                    //        }
                    //        if (foundAction) break;
                    //    }
                    //    // if action wasn't found in this config we remove it
                    //    if (!foundAction)
                    //    {
                    //        allConfigs.RemoveAt(i);
                    //    }
                    //}

                    int priorityValue = 0;

                    //while (unavailablePriorities.Contains(priorityValue))
                    //{
                    //    priorityValue++;
                    //}

                    newAction.FindPropertyRelative("Priority").intValue = priorityValue;
                    AddPriorityFromCount(actionGuid, 0);

                    serializedObject.ApplyModifiedProperties();
                    if (DuplicatePriorityResearch != null)
                    {
                        EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
                        DuplicatePriorityResearch = null;
                    }
                    DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutine(DuplicatePrioritySearchCoroutine(), this);
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
}