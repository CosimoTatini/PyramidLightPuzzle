using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
   public static PlacementManager Instance;
   private Dictionary<Vector3Int,GameObject> _placedItems= new Dictionary<Vector3Int,GameObject>();
   private Dictionary<Tilemap,HashSet<Vector3Int>> _restrictedCells= new Dictionary<Tilemap, HashSet<Vector3Int>>();
   private Dictionary<Vector3Int, TorchType> _torchTypes = new Dictionary<Vector3Int, TorchType>();

    #region SINGLETON_INSTANCE
    private void Awake()
    {
        if(Instance != null && Instance !=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion


    public void SetCellRestriction(Tilemap tilemap,Vector3Int cellPos,bool isRestricted)
    {
        if(!_restrictedCells.ContainsKey(tilemap))
        {
            _restrictedCells[tilemap] = new HashSet<Vector3Int>();

            
        }
        if (isRestricted)
        {
            _restrictedCells[tilemap].Add(cellPos);
        }

        else
        {
            _restrictedCells[tilemap].Remove(cellPos);
        }
    }

    public bool IsCellRestricted(Tilemap tilemap,Vector3Int cellPos)
    {
        if(tilemap == null || !_restrictedCells.ContainsKey(tilemap))
        {
            return false;
        }

        return _restrictedCells[tilemap].Contains(cellPos);
    }

    public bool IsCellAvailable(Tilemap tilemap,Vector3Int cellPos)
    {
        if (_placedItems.ContainsKey(cellPos)) return false;
        if (IsCellRestricted(tilemap, cellPos)) return false;
        return true;
    }

    #region DICTIONARY_METHODS
    public bool IsPossibleToRegisterItem(Tilemap tilemap,Vector3Int cellpos,GameObject item,TorchType type)
    {
        if(!IsCellAvailable(tilemap,cellpos))
        {
            return false;
        }

        _placedItems.Add(cellpos, item);
        _torchTypes.Add(cellpos, type);
        return true;
    }

    public void UnregisterItem(Vector3Int cellpos)
    {
        if(_placedItems.ContainsKey(cellpos))
        {
            _placedItems.Remove(cellpos);
        }
        if(_torchTypes.ContainsKey(cellpos))
        {
            _torchTypes.Remove(cellpos);
        }
    }

    public GameObject GetItemAt(Vector3Int cellPos)
    {
        if (_placedItems.TryGetValue(cellPos, out GameObject item))
        {
            return item;
        }
        return null;
    }
    #endregion

    public KeyValuePair<Vector3Int, GameObject>? FindMagicalTorch()
    {
        foreach (var pair in _torchTypes)
        {
           if(pair.Value==TorchType.Magical)
           {
             Vector3Int cellPos= pair.Key;

                if(_placedItems.TryGetValue(cellPos,out GameObject torch))
                {
                    return new KeyValuePair<Vector3Int, GameObject>(cellPos, torch);
                }
           }
        }
        return null;
    }





}
