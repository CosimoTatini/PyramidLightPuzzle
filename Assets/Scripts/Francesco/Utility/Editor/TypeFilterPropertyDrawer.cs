using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeFilter))]
public class TypeFilterPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty propTypeVar = property.FindPropertyRelative("_typeVar");
        SerializedProperty propStrictMode = property.FindPropertyRelative("_strictMode");

        //if (propTypeVar.objectReferenceValue == null)
        //{
        //    Debug.Log("HE");
        //    using (var check = new EditorGUI.ChangeCheckScope())
        //    {
        //        EditorGUILayout.PropertyField(propTypeVar);
        //        if (check.changed)
        //        {
        //            propTypeVar.serializedObject.ApplyModifiedProperties();
        //        }
        //    }
        //    return;
        //}

        Type type = (propTypeVar.objectReferenceValue) ? ((TypeVar)propTypeVar.objectReferenceValue).Type : null;

        //if (type == null)
        //{
        //    Debug.Log("HEHEHE");

        //    using (var check = new EditorGUI.ChangeCheckScope())
        //    {
        //        EditorGUILayout.PropertyField(propTypeVar);
        //        if (check.changed)
        //        {
        //            propTypeVar.serializedObject.ApplyModifiedProperties();
        //        }
        //    }
        //    return;
        //}

        using (new EditorGUILayout.HorizontalScope())
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propTypeVar);
                bool isInterface = type != null && type.IsInterface;
                using (new EditorGUI.DisabledGroupScope(isInterface || type == null))
                {
                    EditorGUILayout.PropertyField(propStrictMode);
                    // automatically set to false if interface
                    if (isInterface) propStrictMode.boolValue = false;
                }
                
                if (check.changed)
                {
                    propTypeVar.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        if (propTypeVar.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("ERROR: Make sure to put a TypeFilter here", MessageType.Error);
        }
        else if (propTypeVar.objectReferenceValue is TypeVar typeVar && typeVar.Type == null)
        { 
            EditorGUILayout.HelpBox("ERROR: The assigned TypeVar asset is missing a script reference. Open the asset and assign a class.", MessageType.Error);
        }
    }
}
