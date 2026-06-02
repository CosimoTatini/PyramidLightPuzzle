using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

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

    private void OnEnable()
    {
        // Access the private [SerializeField] list from the target ScriptableObject
        _assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");
        _alreadyCreatedArrayElementForAssetMapList = false;
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

    }

    /// <summary>
    /// Updates the serialized object and draws the custom inspector elements, 
    /// including the loader field and the filtered map list.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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
        TypeVar previousTypeVar = (TypeVar)_assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue;
        _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue = (TypeVar)EditorGUILayout.ObjectField("TypeVar input c# script", _assetMapListProp.GetArrayElementAtIndex(assetMapListIndex).FindPropertyRelative("AssetType").objectReferenceValue, typeof(TypeVar), false);

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
        InputActionAsset[] inputActionAssets = AssetDatabaseUtils.GetAssetsByType<InputActionAsset>();

        HashSet<string> mapsWithoutGUID = new();

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
            if (!found) mapsWithoutGUID.Add(guid);
        });

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

        if (mapsWithoutGUID.Count == 0) return;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.HelpBox($"There are {mapsWithoutGUID.Count} maps with missing InputActionAsset", MessageType.Warning);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                LoopThroughMapsReverse((guid, inputAssetMapList, inputAssetMapListIndex, mapIndex) =>
                {
                    SerializedProperty mapStructList = inputAssetMapList.FindPropertyRelative("InputMapStructs");
                    if (mapsWithoutGUID.Contains(guid))
                    {
                        mapStructList.DeleteArrayElementAtIndex(mapIndex);
                        mapsWithoutGUID.Remove(guid);
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
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {
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
        EditorGUILayout.LabelField(action?.name ?? "Unknown", GUILayout.MinWidth(100));
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.Width(60));

        // remove button
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            list.DeleteArrayElementAtIndex(index);
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
                    newAction.FindPropertyRelative("Priority").intValue = 0;
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
}