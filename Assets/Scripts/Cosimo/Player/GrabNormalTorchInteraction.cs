public class GrabNormalTorchInteraction : ItemInteraction
{
    public override void Interact()
    {
        if (_player == null) return;
        if (_itemPlacement == null) return;

        if (TryGetComponent<TypeChooser>(out var torchComponent))
        {
            if (torchComponent.Type == TorchType.Magical) return;

            InventoryManager.Instance.ReturnTorch(torchComponent.Type);
            // PlacementManager.Instance.TryToUnregisterItem(gameObject, PlacementManager.Instance.TargetTilemap);

            if (torchComponent.IsEternal)
            {
                PlacementManager.InvokeEternalTorchRemoved();
                torchComponent.IsEternal = false;
            }

            _player.SetState(ECharacterStates.Grab);
            Destroy(gameObject);
        }
    }
}
