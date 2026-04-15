using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(InterfaceReference<>), true)]

public class InterfaceReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty propGameObject = property.FindPropertyRelative("_gameObject");

        object target = property.serializedObject.targetObject;

        // retrieve the Type of our current child class
        Type instanceType = fieldInfo.FieldType;

        // check if we are inside a List or array, in that case we need to get the passed type to get our actual class InterfaceReference<T>
        // GetGenericTypeDefinition helps us get the "blueprint" instead of GenericClas<T>, so we only get GenericClass<>
        // and we can actually compare, otherwise we would always get false
        if (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == typeof(List<>))
        {
            instanceType = instanceType.GetGenericArguments()[0];
        }
        else if (instanceType.IsArray)
        {
            instanceType = instanceType.GetElementType();
        }

        // search up the inheritance hierarchy to find the generic parent
        Type baseType = instanceType;

        while (baseType != null && !baseType.IsGenericType)
        {
            baseType = baseType.BaseType;
        }

        // we found the generic Parent, we can retrieve the argument type
        if (baseType != null)
        {
            Type argumentType = baseType.GenericTypeArguments[0];
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(propGameObject);
                if (change.changed)
                {
                    GameObject gameObject = propGameObject.objectReferenceValue as GameObject;
                    if (!gameObject) goto SaveChangesToGameObject;

                    Component[] foundComponents = gameObject.GetComponents(argumentType);
                    if (foundComponents == null || foundComponents.Length == 0)
                    {
                        propGameObject.objectReferenceValue = null;
                    }
                    else
                    {
                        foreach (var item in foundComponents)
                        {
                            Debug.Log(item.name);
                        }
                    }
                }

            SaveChangesToGameObject:
                property.serializedObject.ApplyModifiedProperties();
            }

        }
    }

    public Type GetGenericTypeArgument(object obj)
    {
        var type = obj.GetType();

        // Loop up the inheritance chain until we find the generic base
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(InterfaceReference<>))
            {
                return type.GetGenericArguments()[0]; // This returns typeof(MyType)
            }
            type = type.BaseType;
        }
        return null;
    }
}


