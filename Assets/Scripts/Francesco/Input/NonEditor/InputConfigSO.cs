using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "ScriptableObjects/" + nameof(InputConfigSO))]
public class InputConfigSO : ScriptableObject
{
    [SerializeField] private List<InputAssetMapList> _inputAssetMaps = new();

    public IReadOnlyList<InputAssetMapList> GetInputAssetMaps()
    {
        return _inputAssetMaps.AsReadOnly();
    }

    /// <summary>
    /// Given a type of IInputActionCollection2, return the map structs belonging to it
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IReadOnlyList<InputAssetMapList> GetInputAssetMaps(Type type)
    {
        if (!typeof(IInputActionCollection2).IsAssignableFrom(type))
        {
            return Array.Empty<InputAssetMapList>();
        }
        return _inputAssetMaps.Where(assetMapList => assetMapList.AssetType.Type == type).ToList().AsReadOnly();
    }
}

[Serializable]
public struct InputAssetMapList
{
    public TypeVar AssetType;
    public List<InputMapStruct> InputMapStructs;
}