using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InputActionEntry
{
    public static readonly string BUTTON_PLACEHOLDER = "@BUTTON";
    public string Guid;
    public int Priority;
    public bool Enabled;
    public string NameOverride;
    public List<InputPromptScheme> PromptSchemes;
}