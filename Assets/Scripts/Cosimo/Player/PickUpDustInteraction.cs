using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;

public class PickUpDustInteraction : ItemInteraction
{
    public override void Interact()
    {
        if (_player == null) return;
        if (_itemPlacement == null) return;

        if (TryGetComponent<PowderColorChooser>(out var powderData))
        {
            PowderColor color = powderData.Color;
            InventoryManager.Instance.AddPowder(color, 1);
            // PlacementManager.Instance.TryToUnregisterItem(gameObject, _itemPlacement.Tilemap);

            _player.SetState(ECharacterStates.Grab);
            Destroy(gameObject);
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
