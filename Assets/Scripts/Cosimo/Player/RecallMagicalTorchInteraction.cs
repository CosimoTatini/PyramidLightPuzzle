using System.Linq;
using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RecallMagicalTorchInteraction : ItemInteraction
{
    //TODO: change it to false, but still need to implement logic to load this, actually I might need more than 1 script
    // like 1 for when you can grab a torch, 1 for when you can place a torch (to make it easier probably place should be the base actually,
    // so it always displays, even if u can't place it, like u facing a wall, it then gets overriden by grab when we collide we something to grab)
    // _player.AddInteractionEntry(this, true);

    public override void Interact()
    {
        if (_player == null) return;

        GameObject magicalTorch = PlacementManager.Instance.FindMagicalTorch();
        if (magicalTorch == null) return;

        if (magicalTorch.TryGetComponent<TypeChooser>(out var torchComponent))
        {
            if (torchComponent.Type == TorchType.Normal) return;

            InventoryManager.Instance.ReturnTorch(torchComponent.Type);
            if (magicalTorch.TryGetComponent(out LightEmitter lightEmitter))
            {
                if (lightEmitter.RedAmount > 0)
                {
                    InventoryManager.Instance.AddPowder(PowderColor.Red, lightEmitter.RedAmount);
                }

                if (lightEmitter.GreenAmount > 0)
                {
                    InventoryManager.Instance.AddPowder(PowderColor.Green, lightEmitter.GreenAmount);
                }

                if (lightEmitter.BlueAmount > 0)
                {
                    InventoryManager.Instance.AddPowder(PowderColor.Blue, lightEmitter.BlueAmount);
                }
            }

            // PlacementManager.Instance.TryToUnregisterItem(magicalTorch, PlacementManager.Instance.TargetTilemap);

            if (torchComponent.IsEternal)
            {
                torchComponent.IsEternal = false;
            }

            _player.SetState(ECharacterStates.Grab);
            Destroy(magicalTorch);
        }
    }
}
