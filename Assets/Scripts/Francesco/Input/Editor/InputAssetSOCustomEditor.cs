using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputAssetSO))]
public class InputAssetSOCustomEditor : Editor
{
    public readonly string PriorityDB_RelativePath = "ScriptableObjects/Input/PriorityDB.asset";

    private InputActionAsset _currentInputAsset;
    private InputActionMap[] _currentMaps;

    public override void OnInspectorGUI()
    {
         InputSystem_Actions inputActions = new();
         InputSystem_Actions inputActionss = new();
    }
}