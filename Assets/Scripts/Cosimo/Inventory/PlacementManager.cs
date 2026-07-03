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
    private Dictionary<Vector3Int, GameObject> _placedItems = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Tilemap, HashSet<Vector3Int>> _restrictedCells = new Dictionary<Tilemap, HashSet<Vector3Int>>();
    private Dictionary<Vector3Int, TorchType> _torchTypes = new Dictionary<Vector3Int, TorchType>();

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

    private void Start()
    {
        RegisterPreExistentTorches();
    }

    /// <summary>
    /// Player can grab the torches that are put from the editor,
    /// so i need that these torches must be registered all at the begin of the game.
    /// </summary>
    private void RegisterPreExistentTorches()
    {
        TypeChooser[] torchesType = FindObjectsByType<TypeChooser>(FindObjectsSortMode.None);

        foreach (var torch in torchesType)
        {
            Vector3Int cellPos = _targetTilemap.WorldToCell(torch.transform.position);

            if (!_placedItems.ContainsKey(cellPos))
            {
                torch.IsPrexistent = true;
                _placedItems.Add(cellPos, torch.gameObject);
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
        if (_placedItems.ContainsKey(cellPos)) return false;
        if (IsCellRestricted(tilemap, cellPos)) return false;
        return true;
    }

    #region DICTIONARY_METHODS
    /// <summary>
    /// Try to register the items to the dictionaries
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="cellpos"></param>
    /// <param name="item"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsPossibleToRegisterItem(Tilemap tilemap, Vector3Int cellpos, GameObject item, TorchType type)
    {
        if (!IsCellAvailable(tilemap, cellpos))
        {
            return false;
        }
        if (!_placedItems.ContainsKey(cellpos))
            _placedItems.Add(cellpos, item);
        if (!_torchTypes.ContainsKey(cellpos))
            _torchTypes.Add(cellpos, type);
        return true;
    }

    /// <summary>
    /// Unregirester the item from the dictionaries
    /// </summary>
    /// <param name="cellpos"></param>
    public void UnregisterItem(Vector3Int cellpos)
    {
        if (_placedItems.TryGetValue(cellpos, out GameObject item))
        {
            if (item != null)
            {
                if (item.TryGetComponent<TypeChooser>(out var torch))
                {
                    if (torch.IsEternal)
                    {
                        OnEternalTorchRemoved?.Invoke();
                    }

                }
            }
            _placedItems.Remove(cellpos);
        }
    }

    /// <summary>
    /// Retrieves the placed item from the map
    /// </summary>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    public GameObject GetItemAt(Vector3Int cellPos)
    {
        if (_placedItems.TryGetValue(cellPos, out GameObject item))
        {
            return item;
        }
        return null;
    }
    /// <summary>
    /// With this method i can retrieve the magical torch from anywhere
    /// </summary>
    /// <returns></returns>
    public KeyValuePair<Vector3Int, GameObject>? FindMagicalTorch()
    {
        foreach (var pair in _placedItems)
        {

            if (pair.Value != null && pair.Value.TryGetComponent<TypeChooser>(out var torch))
            {

                if (torch.Type == TorchType.Magical && !torch.IsPrexistent)
                {
                    return pair;
                }
            }
        }


        return null;
    }
    #endregion


}







