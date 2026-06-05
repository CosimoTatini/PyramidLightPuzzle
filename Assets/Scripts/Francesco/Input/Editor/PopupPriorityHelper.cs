using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PopupPriorityHelper : PopupWindowContent
{
    private Dictionary<SerializedProperty, InputConfigSO> _actionPriorities;
    Dictionary<int, int> prioritiesCount = new();
    private string _actionName;
    private Vector2 _scrollPos;

    public PopupPriorityHelper(Dictionary<SerializedProperty, InputConfigSO> actionPriorities, string actionName)
    {
        _actionPriorities = actionPriorities;
        _actionName = actionName;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(x: 300, 300);
    }

    public override void OnGUI(Rect rect)
    {
        EditorGUILayout.LabelField(_actionName, GUILayout.MinWidth(100));
        using (var scope = new GUILayout.ScrollViewScope(_scrollPos, false, false, GUILayout.ExpandHeight(true), GUILayout.Height(GetWindowSize().y - EditorGUIUtility.singleLineHeight - 10)))
        {
            _scrollPos = scope.scrollPosition;

            prioritiesCount.Clear();

            // get for each priority value the number of times it's repeated
            foreach (var item in _actionPriorities)
            {
                var actionProp = item.Key;

                int priority = actionProp.FindPropertyRelative("Priority").intValue;
                if (!prioritiesCount.ContainsKey(priority)) prioritiesCount[priority] = 0;
                prioritiesCount[priority]++;

            }
            using (new GUILayout.VerticalScope(GUILayout.Width(GetWindowSize().x - 30)))
            {
                // draw action rows, but instead of the name of the action visualize its configSO
                foreach (var item in _actionPriorities)
                {
                    var actionProp = item.Key;
                    var configSO = item.Value;

                    int priority = actionProp.FindPropertyRelative("Priority").intValue;

                    Color guiColor = GUI.backgroundColor;

                    if (prioritiesCount[priority] > 1)
                    {
                        GUI.backgroundColor = Color.yellowNice;
                    }
                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ObjectField("", configSO, typeof(InputConfigSO), false, GUILayout.MaxWidth(150));
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Enabled"), GUIContent.none, GUILayout.Width(40));
                    EditorGUILayout.PropertyField(actionProp.FindPropertyRelative("Priority"), GUIContent.none, GUILayout.Width(60));
                    if (actionProp.serializedObject.hasModifiedProperties)
                    {
                        actionProp.serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(3);

                    GUI.backgroundColor = guiColor;
                }
            }
        }
    }
}
