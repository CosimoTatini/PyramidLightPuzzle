using UnityEngine;

public class GrabNormalTorchInteraction : ItemInteraction
{
    public override void Interact()
    {
        if (_player == null) return;
        if (_itemPlacement == null) return;

        _player.SetState(ECharacterStates.Grab);
        Destroy(gameObject);

        // if (TryGetComponent<TypeChooser>(out var torchComponent))
        // {
        //     if (torchComponent.Type == TorchType.Magical) return;

        //     InventoryManager.Instance.ReturnTorch(torchComponent.Type);

        //     if (torchComponent.IsEternal)
        //     {
        //         PlacementManager.InvokeEternalTorchRemoved();
        //         torchComponent.IsEternal = false;
        //     }

        //     _player.SetState(ECharacterStates.Grab);
        //     Destroy(gameObject);
        // }
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
