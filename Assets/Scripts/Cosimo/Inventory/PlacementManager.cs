using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
   public static PlacementManager Instance;
   private Dictionary<Vector3Int,GameObject> _placedItems= new Dictionary<Vector3Int,GameObject>();

    private void Awake()
    {
        if(Instance != null && Instance !=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsCellAvailable(Vector3Int cellPos)
    {
      return !_placedItems.ContainsKey(cellPos);
    }

    public bool IsPossibleToRegisterItem(Vector3Int cellpos,GameObject item)
    {
        if(!IsCellAvailable(cellpos))
        {
            return false;
        }

        _placedItems.Add(cellpos, item);
        return true;
    }

    public void UnregisterItem(Vector3Int cellpos)
    {
        if(_placedItems.ContainsKey(cellpos))
        {
            _placedItems.Remove(cellpos);
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


}
