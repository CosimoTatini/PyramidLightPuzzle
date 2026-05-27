using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Custom Inspector for <see cref="InputConfigSO"/>.
/// Provides a specialized interface for managing Input Map and Action overrides 
/// by resolving GUIDs through a temporary reference to an <see cref="InputActionAsset"/>.
/// </summary>
[CustomEditor(typeof(InputConfigSO))]
public class InputConfigSOEditor : Editor
{
    private InputActionAsset _loaderAsset;
    private SerializedProperty _mapListProp;
    
    //TODO: make SO database for priorities, so we can assign a unique priority value to each action inside of a config

    private void OnEnable()
    {
        // Access the private [SerializeField] list from the target ScriptableObject
        _mapListProp = serializedObject.FindProperty("_inputMapStructs");
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
        _loaderAsset = (InputActionAsset)EditorGUILayout.ObjectField(
            "Reference Asset", _loaderAsset, typeof(InputActionAsset), false);

        if (_loaderAsset == null)
        {
            EditorGUILayout.HelpBox("Assign an Input Asset to edit overrides.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();

        // 2. Add Map Button ➕
        DrawAddMapMenu();

        EditorGUILayout.Space();

        // 3. Draw Filtered Maps
        // Only render maps that exist in this InputConfigSO and that are present in the assigned loader asset
        // this is the filter step for ensuring we can only add/show maps that are present in this loader asset

        // get all of the guids of the maps in the current loader asset
        var assetMapGuids = _loaderAsset.actionMaps.Select(m => m.id.ToString()).ToHashSet();

        // cycle through the list of maps of this SO and only draw the ones that are present in the assigned loader asset
        for (int i = 0; i < _mapListProp.arraySize; i++)
        {
            SerializedProperty mapElem = _mapListProp.GetArrayElementAtIndex(i);
            string guid = mapElem.FindPropertyRelative("Guid").stringValue;

            if (assetMapGuids.Contains(guid))
            {
                DrawMapFoldout(mapElem, i);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draws a foldout for a specific Input Map, displaying its action overrides 
    /// and providing options to add or remove overrides.
    /// </summary>
    private void DrawMapFoldout(SerializedProperty mapProp, int index)
    {
        string guid = mapProp.FindPropertyRelative("Guid").stringValue;
        var assetMap = _loaderAsset.actionMaps.First(m => m.id.ToString() == guid);

        EditorGUILayout.BeginVertical("helpbox");

        // Header Row with Map Name and Remove Button
        EditorGUILayout.BeginHorizontal();
        mapProp.isExpanded = EditorGUILayout.Foldout(mapProp.isExpanded, $"Map: {assetMap.name}", true);

        // delete option before drawing the map to avoid crashing when we delete an element that is currently being drawn
        if (GUILayout.Button("Remove Map", GUILayout.Width(100)))
        {
            _mapListProp.DeleteArrayElementAtIndex(index);
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
            EditorGUILayout.LabelField("On", EditorStyles.miniBoldLabel, GUILayout.Width(30));
            EditorGUILayout.LabelField("Priority", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            // Matches the field name in InputMapStruct
            SerializedProperty actionsList = mapProp.FindPropertyRelative("InputActionStructs");

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
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(30));
        EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.Width(50));

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
    private void DrawAddMapMenu()
    {
        if (GUILayout.Button("Add Map Override..."))
        {
            GenericMenu menu = new();

            // get the existing map guids in this so to avoid adding duplicates to the menu
            var existingGuids = GetExistingGuids(_mapListProp, "Guid");

            // loop through the maps of the loader asset and only add to the menu the ones that are not already present in this SO
            foreach (var map in _loaderAsset.actionMaps)
            {
                string mapGuid = map.id.ToString();
                if (existingGuids.Contains(mapGuid)) continue;

                // add item to menu, when item is clicked we add to map list a new element with the guid of the selected map
                menu.AddItem(new GUIContent(map.name), false, () => {
                    // add new item with the next index available at the end of the list
                    int index = _mapListProp.arraySize;
                    _mapListProp.InsertArrayElementAtIndex(index);

                    // set the guid of the new map element to the guid of the current map
                    var newMap = _mapListProp.GetArrayElementAtIndex(index);
                    newMap.FindPropertyRelative("Guid").stringValue = mapGuid;

                    // reset the cloned action list to ensure a clean slate, otherwise it would try to copy the actions of the last element in the list
                    var newActionsList = newMap.FindPropertyRelative("InputActionStructs");
                    newActionsList.ClearArray();

                    // expand by default
                    newMap.isExpanded = true;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            if(menu.GetItemCount() == 0)
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

                menu.AddItem(new GUIContent(action.name), false, () => {
                    int index = actionsList.arraySize;
                    actionsList.InsertArrayElementAtIndex(index);
                    var newAction = actionsList.GetArrayElementAtIndex(index);
                    newAction.FindPropertyRelative("Guid").stringValue = actionGuid;
                    newAction.FindPropertyRelative("Enabled").boolValue = true;
                    newAction.FindPropertyRelative("Priority").intValue = 0;
                    serializedObject.ApplyModifiedProperties();
                });
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
}