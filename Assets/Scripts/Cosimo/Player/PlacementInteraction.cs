using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementInteraction : PriorityInteractable
{
    [SerializeField] private Player _player;

    private void Awake()
    {
        //TODO: change it to false, but still need to implement logic to load this, actually I might need more than 1 script
        // like 1 for when you can grab a torch, 1 for when you can place a torch (to make it easier probably place should be the base actually,
        // so it always displays, even if u can't place it, like u facing a wall, it then gets overriden by grab when we collide we something to grab)
        _player.AddInteractionEntry(this, true);
    }

    public override void Interact()
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

        _player.SetState(ECharacterStates.Place);
    }
}
