using System.Linq;
using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RecallMagicalTorchInteraction : ItemInteraction
{
    public override void Interact()
    {
        if (_player == null) return;

        GameObject magicalTorch = PlacementManager.Instance.FindItemOfType(typeof(MagicalTorch));
        if (magicalTorch == null) return;

        OnInteract.Invoke();
        _player.SetState(ECharacterStates.Grab);
        Destroy(magicalTorch);

        // if (magicalTorch.TryGetComponent<TypeChooser>(out var torchComponent))
        // {
        //     if (torchComponent.Type == TorchType.Normal) return;

        //     InventoryManager.Instance.ReturnTorch(torchComponent.Type);
        //     if (magicalTorch.TryGetComponent(out LightEmitter lightEmitter))
        //     {
        //         if (lightEmitter.RedAmount > 0)
        //         {
        //             InventoryManager.Instance.AddPowder(PowderColor.Red, lightEmitter.RedAmount);
        //         }

        //         if (lightEmitter.GreenAmount > 0)
        //         {
        //             InventoryManager.Instance.AddPowder(PowderColor.Green, lightEmitter.GreenAmount);
        //         }

        //         if (lightEmitter.BlueAmount > 0)
        //         {
        //             InventoryManager.Instance.AddPowder(PowderColor.Blue, lightEmitter.BlueAmount);
        //         }
        //     }

        //     // PlacementManager.Instance.TryToUnregisterItem(magicalTorch, PlacementManager.Instance.TargetTilemap);

        //     if (torchComponent.IsEternal)
        //     {
        //         torchComponent.IsEternal = false;
        //     }

        //     _player.SetState(ECharacterStates.Grab);
        //     Destroy(magicalTorch);
        // }
    }
}
