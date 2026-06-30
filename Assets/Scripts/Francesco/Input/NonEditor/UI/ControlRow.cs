using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private List<string> _keysValues;
    [SerializeField] private string _description;

    public void Initialize(string newString)
    {
        _textField.text = newString;
    }
}