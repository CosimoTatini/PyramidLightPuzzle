using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;

    public void Initialize(string newString)
    {
        _textField.text = newString;
    }
}