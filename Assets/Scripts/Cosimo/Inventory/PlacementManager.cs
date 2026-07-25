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
    
    [Header("Debug")]
    [SerializeField] private bool _visualizedGrid = true;
    private Dictionary<Tilemap, Dictionary<Vector3Int, GameObject>> _tilemapCellsOccupied = new();
    private Dictionary<Tilemap, Dictionary<GameObject, Vector3Int[]>> _tilemapItemsCells = new();
    private Dictionary<Tilemap, HashSet<Vector3Int>> _restrictedCells = new Dictionary<Tilemap, HashSet<Vector3Int>>();
    private Dictionary<Vector3Int, TorchType> _torchTypes = new Dictionary<Vector3Int, TorchType>();
    public static event Action OnSceneLoaded;

    public static void NotifySceneLoaded()
    {
        OnSceneLoaded?.Invoke();
    }
    private void OnEnable()
    {
        OnSceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        OnSceneLoaded -= HandleSceneLoaded;
    }
    private void HandleSceneLoaded()
    {
        RegisterPreExistentTorches();
    }

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

    public static void InvokeEternalTorchRemoved()
    {
        OnEternalTorchRemoved?.Invoke();
    }

    /// <summary>
    /// Player can grab the torches that are put from the editor,
    /// so i need that these torches must be registered all at the begin of the game.
    /// </summary>
    /// 
    public void RegisterPreExistentTorches()
    {
        // TypeChooser[] torchesType = FindObjectsByType<TypeChooser>(FindObjectsSortMode.None);

        // foreach (var torch in torchesType)
        // {
        //     Vector3Int cellPos = _targetTilemap.WorldToCell(torch.transform.position);

        //     if (!_cellsOccupied.ContainsKey(cellPos))
        //     {
        //         torch.IsEternal = true;
        //         _cellsOccupied.Add(cellPos, torch.gameObject);
        //     }
        //     else
        //     {
        //         Debug.Log("Pippo");
        //     }
        // }
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
        if (!_tilemapCellsOccupied.ContainsKey(tilemap)) return true;
        if (_tilemapCellsOccupied[tilemap].ContainsKey(cellPos)) return false;
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
        return TryToRegisterItem(tilemap, new Vector3Int[] { neededCell }, item);
    }

    public bool TryToRegisterItem(Tilemap tilemap, Vector3Int[] neededCells, GameObject item)
    {
        if (item == null || tilemap == null) return false;
        if (neededCells == null || neededCells.Length == 0) return false;

        for (int i = 0; i < neededCells.Length; i++)
        {
            Vector3Int cell = neededCells[i];
            if (!IsCellAvailable(tilemap, cell))
            {
                return false;
            }
        }

        if (!_tilemapItemsCells.ContainsKey(tilemap))
        {
            _tilemapItemsCells[tilemap] = new();
        }

        var itemsCells = _tilemapItemsCells[tilemap];

        if (!itemsCells.ContainsKey(item))
        {
            itemsCells[item] = neededCells;
        }
        else
        {
            return false;
        }

        if (!_tilemapCellsOccupied.ContainsKey(tilemap))
        {
            _tilemapCellsOccupied[tilemap] = new();
        }

        for (int i = 0; i < neededCells.Length; i++)
        {
            Vector3Int cell = neededCells[i];
            _tilemapCellsOccupied[tilemap].Add(cell, item);
        }

        return true;
    }

    /// <summary>
    /// Unregirester the item from the dictionaries
    /// </summary>
    /// <param name="cellpos"></param>
    public bool TryToUnregisterItem(Vector3Int cellpos, Tilemap tilemap)
    {
        if (_tilemapCellsOccupied.ContainsKey(tilemap) && _tilemapCellsOccupied[tilemap].TryGetValue(cellpos, out GameObject item))
        {
            return TryToUnregisterItem(item, tilemap);
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

    public bool TryToUnregisterItem(GameObject gameObject, Tilemap tilemap)
    {
        if (gameObject == null || tilemap == null) return false;
        if (_tilemapItemsCells.ContainsKey(tilemap) && _tilemapItemsCells[tilemap].TryGetValue(gameObject, out var occupiedCells))
        {
            if (!_tilemapCellsOccupied.ContainsKey(tilemap)) return false;

            for (int i = 0; i < occupiedCells.Length; i++)
            {
                _tilemapCellsOccupied[tilemap].Remove(occupiedCells[i]);
            }
            _tilemapItemsCells[tilemap].Remove(gameObject);
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
    public GameObject GetItemAt(Vector3Int cellPos, Tilemap tilemap)
    {
        if (_tilemapCellsOccupied.ContainsKey(tilemap) && _tilemapCellsOccupied[tilemap].TryGetValue(cellPos, out GameObject item))
        {
            return item;
        }
        return null;
    }

    public bool HasItem(GameObject gameObject)
    {
        if (gameObject == null) return false;

        foreach (var itemsCells in _tilemapItemsCells.Values)
        {
            foreach (var item in itemsCells.Keys)
            {
                if (item == gameObject) return true;
            }
        }

        return false;
    }
    /// <summary>
    /// With this method i can retrieve the magical torch from anywhere
    /// </summary>
    /// <returns></returns>
    public GameObject FindItemOfType(Type type)
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

        foreach (var itemsCells in _tilemapItemsCells.Values)
        {
            foreach (var item in itemsCells.Keys)
            {
                if (item == null) continue;
                if (item.TryGetComponent(type, out var component))
                {
                    return item;
                }
            }
        }

        return null;
    }

    public GameObject[] FindItemsOfType(Type type)
    {
        List<GameObject> itemsFound = new();

        foreach (var itemsCells in _tilemapItemsCells.Values)
        {
            foreach (var item in itemsCells.Keys)
            {
                if (item == null) continue;
                if (item.TryGetComponent(type, out var component))
                {
                    itemsFound.Add(item);
                }
            }
        }

        return itemsFound.ToArray();
    }

    public void InitializeTilemap(Tilemap tilemap)
    {
        _targetTilemap = tilemap;
    }

    #endregion

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || _targetTilemap == null || !_visualizedGrid) return;

        BoundsInt bounds = _targetTilemap.cellBounds;
        TileBase[] allTiles = _targetTilemap.GetTilesBlock(bounds);

        Color gizmoColor = Color.lightGreen;
        gizmoColor.a = 0.4f;
        Gizmos.color = gizmoColor;

        for (int i = 0; i < allTiles.Length; i++)
        {
            TileBase tile = allTiles[i];
            if (tile != null)
            {
                int x = (i % bounds.size.x) + bounds.xMin;
                int y = (i / bounds.size.x) + bounds.yMin;
                Vector3Int position = new Vector3Int(x, y, bounds.zMin);
                if (IsCellAvailable(_targetTilemap, position))
                    Gizmos.DrawCube(position + new Vector3(0.5f, 0.5f, 0f), Vector3Int.one - new Vector3(0.1f, 0.1f, 0));
            }

        }
    }
}