using log4net.Util;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementInteraction : MonoBehaviour, IPriorityInteractable
{
    [SerializeField] private Player _player;
    [field: SerializeField] public InputConfigSO InputConfigSO { get; }

    private void Awake()
    {
        _player.AddInteractionEntry(this);
    }

    public InputActionEntry GetFirstEntry()
    {
        if(InputConfigSO == null) return null;

        var allConfigActionsGuids = InputConfigSO.GetInputAssetMaps()
        .SelectMany(k => k.InputMapStructs)
        .SelectMany(k => k.InputActionEntries);

        if (allConfigActionsGuids != null && allConfigActionsGuids.Count() > 0)
        {
            return allConfigActionsGuids.ElementAt(0);
        }
        else
        {
            return null;
        }
    }

    public void Interact()
    {
        if (InventoryManager.Instance.SelectedType == TorchType.Magical)
        {
            if (PlacementManager.Instance.FindMagicalTorch().HasValue)
            {
                _player.SetState(ECharacterStates.Grab);
                return;
            }
        }

        Tilemap placeableTilemap = PlacementManager.Instance.TargetTilemap;
        Vector3Int currentCellPos = placeableTilemap.WorldToCell(_player.transform.position);

        if (!PlacementManager.Instance.IsCellAvailable(placeableTilemap, currentCellPos))
        {
            if (InventoryManager.Instance.SelectedType == TorchType.Normal)
            {
                _player.SetState(ECharacterStates.Grab);
                return;
            }
        }
        Vector3 targetWorldPos = _player.transform.position + (Vector3) _player.PlayerController.LastLookDirection * _player.CellOffset;
        Vector3Int forwardCellPos = placeableTilemap.WorldToCell(targetWorldPos);

        if (!PlacementManager.Instance.IsCellAvailable(placeableTilemap, forwardCellPos))
        {
            Vector3 cellCenter = placeableTilemap.GetCellCenterWorld(forwardCellPos);

            if (Vector2.Distance(_player.transform.position, cellCenter) <= 0.6f)
            {
                _player.SetState(ECharacterStates.Grab);
                return;
            }
        }
    }
}
