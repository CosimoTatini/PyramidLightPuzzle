using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/"+ nameof(InputConfigSO))]
public class InputConfigSO : ScriptableObject
{
    [SerializeField] private List<InputMapStruct> _inputMapStructs = new();

    public IReadOnlyList<InputMapStruct> GetInputMapStructs()
    {
        return _inputMapStructs.AsReadOnly();
    }

    public IReadOnlyList<InputMapStruct> GetInputMapStructs(InputActionAsset inputAsset)
    {
        return _inputMapStructs.Where(map => inputAsset.FindActionMap(map.Guid) != null).ToList().AsReadOnly();
    }
}