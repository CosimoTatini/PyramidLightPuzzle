using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(InputMapStruct))]
public class InputMapStructEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new();
        SerializedProperty entries = property.FindPropertyRelative("InputActionEntries");
        PropertyField entriesField = new(entries);
        root.Add(entriesField);
        
        return root;
    }
}