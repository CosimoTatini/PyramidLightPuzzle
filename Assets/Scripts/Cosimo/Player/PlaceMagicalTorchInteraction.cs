using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceMagicalTorchInteraction : PlayerPriorityInteractable
{
    [SerializeField] private AudioClipListGroup _audioClipListGroup;

    public override void Interact()
    {
        if (_player == null) return;

        if (!InventoryManager.Instance.CanPlace())
        {
            return;
        }

        Tilemap groundTilemap = PlacementManager.Instance.TargetTilemap;

        if (groundTilemap == null)
        {
            return;
        }

        // Vector2 look = _player.PlayerController.LastLookDirection;
        // Vector3 interactionPos = _player.transform.position + (Vector3)look * 0.5f;
        Vector3 interactionPos = _player.FeetTransform.position;
        Vector3Int cellPos = groundTilemap.WorldToCell(interactionPos);

        if (!groundTilemap.HasTile(cellPos))
        {
            return;
        }

        if (!PlacementManager.Instance.IsCellAvailable(PlacementManager.Instance.TargetTilemap, cellPos))
        {
            return;
        }

        Vector3 spawnWorldPos = groundTilemap.GetCellCenterWorld(cellPos);

        TorchType type = InventoryManager.Instance.SelectedType;
        GameObject torchInstance = Instantiate(InventoryManager.Instance.MagicalTorchPrefab, spawnWorldPos, Quaternion.identity);
        if (torchInstance.TryGetComponent(out ItemPlacement itemPlacement))
        {
            itemPlacement.Tilemap = groundTilemap;
        }

        if (PlacementManager.Instance.HasItem(torchInstance))
        {
            Global2DAudioPlayer.Instance.PlayOneShotRandom(_audioClipListGroup);
            _player.SetState(ECharacterStates.Place);
        }
        else
        {
            Destroy(torchInstance);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.AddInteractable(this);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.RemoveInteractable(this);
        }
    }
}
