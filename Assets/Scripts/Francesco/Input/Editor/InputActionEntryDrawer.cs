using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(InputActionEntry))]
public class InputActionEntryDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new();
        root.style.flexDirection = FlexDirection.Row;
        root.style.flexWrap = Wrap.NoWrap;
        root.style.justifyContent = Justify.SpaceBetween;

        IntegerField priorityField = new("Priority");
        priorityField.bindingPath = "Priority";

        priorityField.style.flexBasis = Length.Percent(40);

        // Target the internal label using the standard USS class
        Label priorityLabel = priorityField.Q<Label>(className: "unity-base-field__label");
        if (priorityLabel != null)
        {
            priorityLabel.style.width = 60;
            priorityLabel.style.minWidth = 60;
            priorityLabel.style.flexShrink = 0;
        }

        Toggle enabladToggle = new("Enabled");
        enabladToggle.bindingPath = "Enabled";
        enabladToggle.style.marginLeft = 5;

        Label enabledLabel = enabladToggle.Q<Label>(className: "unity-base-field__label");
        if (enabledLabel != null)
        {
            enabledLabel.style.width = 60;
            enabledLabel.style.minWidth = 60;
            enabledLabel.style.flexShrink = 0;
        }

        enabladToggle.style.flexBasis = Length.Percent(60);

        root.Add(priorityField);
        root.Add(enabladToggle);
        return root;
    }
}