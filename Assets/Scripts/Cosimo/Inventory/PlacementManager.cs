using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Placement manager handles the Place/Grab interactions of the player.
/// </summary>
public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;
    public static event Action OnEternalTorchRemoved;
    [SerializeField] private Tilemap _targetTilemap;
    private Dictionary<Vector3Int, GameObject> _cellsOccupied = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<GameObject, Vector3Int[]> _itemsCells = new();
    private Dictionary<Tilemap, HashSet<Vector3Int>> _restrictedCells = new Dictionary<Tilemap, HashSet<Vector3Int>>();
    private Dictionary<Vector3Int, TorchType> _torchTypes = new Dictionary<Vector3Int, TorchType>();

    public Tilemap TargetTilemap
    {
        get => _targetTilemap;
        set => _targetTilemap = value;
    }

    #region SINGLETON_INSTANCE
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    #endregion

    /// <summary>
    /// Player can grab the torches that are put from the editor,
    /// so i need that these torches must be registered all at the begin of the game.
    /// </summary>
    public void RegisterPreExistentTorches()
    {
        TypeChooser[] torchesType = FindObjectsByType<TypeChooser>(FindObjectsSortMode.None);

        foreach (var torch in torchesType)
        {
            Vector3Int cellPos = _targetTilemap.WorldToCell(torch.transform.position);
            Debug.Log($"[PlacementManager] Tento di registrare la torcia '{torch.name}' alla cella: {cellPos}");

            if (!_cellsOccupied.ContainsKey(cellPos))
            {
                torch.IsEternal = true;
                _cellsOccupied.Add(cellPos, torch.gameObject);
            }
            else
            {
                Debug.Log("Pippo");
            }
        }
    }

    /// <summary>
    /// With this method i can restrict some cells for a limited time.
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="cellPos"></param>
    /// <param name="isRestricted"></param>
    public void SetCellRestriction(Tilemap tilemap, Vector3Int cellPos, bool isRestricted)
    {
        if (!_restrictedCells.ContainsKey(tilemap))
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

    /// <summary>
    /// Toggle up the restricted flag.
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    public bool IsCellRestricted(Tilemap tilemap, Vector3Int cellPos)
    {
        if (tilemap == null || !_restrictedCells.ContainsKey(tilemap))
        {
            return false;
        }

        return _restrictedCells[tilemap].Contains(cellPos);
    }
    /// <summary>
    /// This extend the restriction, it verifies that the cell is available
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    public bool IsCellAvailable(Tilemap tilemap, Vector3Int cellPos)
    {
        if (_cellsOccupied.ContainsKey(cellPos)) return false;
        if (IsCellRestricted(tilemap, cellPos)) return false;
        return true;
    }

    #region DICTIONARY_METHODS
    /// <summary>
    /// Try to register the items to the dictionaries
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="neededCells"></param>
    /// <param name="item"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool TryToRegisterItem(Tilemap tilemap, Vector3Int neededCell, GameObject item)
    {
        if (item == null) return false;

        if (!IsCellAvailable(tilemap, neededCell))
        {
            return false;
        }

        if (!_itemsCells.ContainsKey(item))
        {
            _itemsCells[item] = new Vector3Int[] { neededCell };
        }
        else
        {
            return false;
        }

        _cellsOccupied.Add(neededCell, item);
        return true;
    }

    public bool TryToRegisterItem(Tilemap tilemap, Vector3Int[] neededCells, GameObject item)
    {
        if (item == null) return false;
        if (neededCells == null || neededCells.Length == 0) return false;

        for (int i = 0; i < neededCells.Length; i++)
        {
            Vector3Int cell = neededCells[i];
            if (!IsCellAvailable(tilemap, cell))
            {
                return false;
            }
        }

        if (!_itemsCells.ContainsKey(item))
        {
            _itemsCells[item] = neededCells;
        }
        else
        {
            return false;
        }

        for (int i = 0; i < neededCells.Length; i++)
        {
            Vector3Int cell = neededCells[i];
            _cellsOccupied.Add(cell, item);
        }

        return true;
    }

    /// <summary>
    /// Unregirester the item from the dictionaries
    /// </summary>
    /// <param name="cellpos"></param>
    public bool TryToUnregisterItem(Vector3Int cellpos)
    {
        if (_cellsOccupied.TryGetValue(cellpos, out GameObject item))
        {
            //TODO: Logic for eternal torch removed should be in the grab Interaction script for magical torch
            // or maybe just OnDisable => if(_isEternal) Invoke;
            // if (item != null)
            // {
            //     if (item.TryGetComponent<TypeChooser>(out var torch))
            //     {
            //         if (torch.IsEternal)
            //         {
            //             OnEternalTorchRemoved?.Invoke();
            //         }

            //     }
            // }
            return TryToUnregisterItem(item);
            // Vector3Int[] occupiedCells = _itemsCells.ContainsKey(item) ? _itemsCells[item] : Array.Empty<Vector3Int>();
            // for (int i = 0; i < occupiedCells.Length; i++)
            // {
            //     _cellsOccupied.Remove(occupiedCells[i]);
            // }
            // _itemsCells.Remove(item);
            // return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryToUnregisterItem(GameObject gameObject)
    {
        if (_itemsCells.TryGetValue(gameObject, out var occupiedCells))
        {
            for (int i = 0; i < occupiedCells.Length; i++)
            {
                _cellsOccupied.Remove(occupiedCells[i]);
            }
            _itemsCells.Remove(gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Retrieves the placed item from the map
    /// </summary>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    public GameObject GetItemAt(Vector3Int cellPos)
    {
        if (_cellsOccupied.TryGetValue(cellPos, out GameObject item))
        {
            return item;
        }
        return null;
    }
    /// <summary>
    /// With this method i can retrieve the magical torch from anywhere
    /// </summary>
    /// <returns></returns>
    public GameObject FindMagicalTorch()
    {
        // foreach (var pair in _cellsOccupied)
        // {
        //     if (pair.Value != null && pair.Value.TryGetComponent<TypeChooser>(out var torch))
        //     {

        //         if (torch.Type == TorchType.Magical && !torch.IsPrexistent)
        //         {
        //             return pair;
        //         }
        //     }
        // }

        foreach (var item in _itemsCells.Keys)
        {
            if (item.TryGetComponent(out TypeChooser torch))
            {
                if (torch.Type == TorchType.Magical && !torch.IsEternal)
                {
                    return item;
                }
            }
        }

        return null;
    }
    public void InitializeTilemap(Tilemap tilemap)
    {
        _targetTilemap = tilemap;
        RegisterPreExistentTorches();
    }

    #endregion


}