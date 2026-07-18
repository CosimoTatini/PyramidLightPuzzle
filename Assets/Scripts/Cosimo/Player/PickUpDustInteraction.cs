using Assets.Scripts.Cosimo.Inventory;

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
}
