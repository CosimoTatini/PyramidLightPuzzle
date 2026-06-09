using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

public class PopupPriorityHelper : PopupWindowContent
{
    private Dictionary<InputConfigSO, string> _actionPrioritiesPaths;
    private List<PriorityRow> _prioritiesRows = new();
    private Dictionary<int, int> _prioritiesCount = new();
    private string _actionName;
    private Vector2 _scrollPos;

    public PopupPriorityHelper(Dictionary<InputConfigSO, string> actionPrioritiesPaths, string actionName)
    {
        _actionPrioritiesPaths = actionPrioritiesPaths;
        _actionName = actionName;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(x: 350, 300);
    }

    private class PriorityRow
    {
        public string Guid;
        public InputConfigSO ConfigSO;
        public SerializedObject SerializedObject;
        public SerializedProperty EnabledProperty;
        public SerializedProperty PriorityProperty;
        public string PropertyPath;
    }

    private string _searchQuery;

    public override void OnGUI(Rect rect)
    {
        EditorGUILayout.LabelField(_actionName, GUILayout.MinWidth(100));
        _searchQuery = EditorGUILayout.TextField("Config Name:", _searchQuery);
        using (var scope = new GUILayout.ScrollViewScope(_scrollPos, false, false, GUILayout.ExpandHeight(true), GUILayout.Height(GetWindowSize().y - EditorGUIUtility.singleLineHeight * 3)))
        {
            _scrollPos = scope.scrollPosition;

            _prioritiesCount.Clear();
            _prioritiesRows.Clear();

            // get for each priority value the number of times it's repeated
            foreach (var item in _actionPrioritiesPaths)
            {
                if (item.Key == null) continue;
                if (!string.IsNullOrEmpty(_searchQuery))
                {
                    if (item.Key.name.IndexOf(_searchQuery, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                }

                SerializedObject inputConfigObject = new(item.Key);
                inputConfigObject.Update();

                var actionProp = inputConfigObject.FindProperty(item.Value);
                if (actionProp == null) continue;

                int priority = actionProp.FindPropertyRelative("Priority").intValue;
                if (!_prioritiesCount.ContainsKey(priority)) _prioritiesCount[priority] = 0;
                _prioritiesCount[priority]++;

                _prioritiesRows.Add(new()
                {
                    ConfigSO = item.Key,
                    Guid = actionProp.FindPropertyRelative("Guid").stringValue,
                    EnabledProperty = actionProp.FindPropertyRelative("Enabled"),
                    PriorityProperty = actionProp.FindPropertyRelative("Priority"),
                    PropertyPath = actionProp.propertyPath,
                    SerializedObject = inputConfigObject
                });
            }

            using (new GUILayout.VerticalScope(GUILayout.Width(GetWindowSize().x - 20)))
            {
                // draw action rows, but instead of the name of the action visualize its configSO
                foreach (var row in _prioritiesRows)
                {
                    row.SerializedObject.Update();

                    int oldPriority = row.PriorityProperty.intValue;

                    Color guiColor = GUI.color;

                    if (_prioritiesCount[oldPriority] > 1)
                    {
                        GUI.color = Color.Lerp(guiColor, Color.yellow, 0.4f);
                    }

                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ObjectField(GUIContent.none, row.ConfigSO, typeof(InputConfigSO), false, GUILayout.MaxWidth(220));
                    }
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(row.EnabledProperty, GUIContent.none, GUILayout.Width(40));
                    EditorGUILayout.PropertyField(row.PriorityProperty, GUIContent.none, GUILayout.Width(60));

                    if (EditorGUI.EndChangeCheck())
                    {
                        row.SerializedObject.ApplyModifiedProperties();

                        int newPriority = row.PriorityProperty.intValue;
                        if (oldPriority != newPriority)
                            InputConfigPriorityCache.RebuildPriorityDictionary();
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(3);

                    GUI.color = guiColor;
                }
            }
        }
    }
}
