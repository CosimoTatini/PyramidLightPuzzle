using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;

public class ThrowDustInteraction : ItemInteraction
{
    [SerializeField] private AudioClipListGroup _audioClipListGroup;

    public override void Interact()
    {
        if (_player == null) return;
        if (_itemPlacement == null) return;

        Debug.Log("THROWING");
        if (!InventoryManager.Instance.CanThrowPowder())
        {
            return;
        }

        GameObject magicalTorch = PlacementManager.Instance.FindItemOfType(typeof(MagicalTorch));
        if (magicalTorch == null)
        {
            return;
        }

        if (!_player.PlayerController.Rb.IsTouching(_itemPlacement.Collider2D))
        {
            return;
        }

        if (!magicalTorch.TryGetComponent(out LightEmitter lightEmitter))
        {
            return;
        }

        PowderColor selectedColor = InventoryManager.Instance.SelectedPowder;

        if (HasRoomForPowder(lightEmitter, selectedColor))
        {
            InventoryManager.Instance.UsePowder();
            ApplyPowderToEmitter(lightEmitter, selectedColor);
            Global2DAudioPlayer.Instance.PlayOneShotRandom(_audioClipListGroup);
            _player.SetState(ECharacterStates.Throw);
        }
    }

    private bool HasRoomForPowder(LightEmitter emitter, PowderColor selectedColor)
    {
        // Verifichiamo immediatamente se l'oggetto è nullo prima di accedere alle sue proprietà
        if (emitter == null)
        {
            Debug.LogError("[Throw - CRITICAL] L'emitter passato a HasRoomForPowder è NULL! Il controllo di vicinanza ha fallito l'assegnazione.");
            return false;
        }

        return selectedColor switch
        {
            PowderColor.Red => emitter.RedAmount < emitter.MaxAmount,
            PowderColor.Green => emitter.GreenAmount < emitter.MaxAmount,
            PowderColor.Blue => emitter.BlueAmount < emitter.MaxAmount,
            _ => false
        };
    }

    private void ApplyPowderToEmitter(LightEmitter emitter, PowderColor selectedColor)
    {
        switch (selectedColor)
        {
            case PowderColor.Red:
                emitter.RedAmount++;
                Debug.Log("[Throw]:Aumentato di 1 il valore di rosso");
                break;
            case PowderColor.Green:
                emitter.GreenAmount++;
                Debug.Log("[Throw]:Aumentato di 1 il valore di verde");
                break;
            case PowderColor.Blue:
                Debug.Log("[Throw]:Aumentato di 1 il valore di blue");
                emitter.BlueAmount++; break;
        }
    }

    private IPriorityInteractableHost _host = null;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_itemPlacement == null || _itemPlacement.Collider2D == null || !collision.IsTouching(_itemPlacement.Collider2D)) return;
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            _host = host;
            host.AddInteractable(this);
            InventoryManager.Instance.OnPowderChanged += PowderChanged;
            PowderChanged();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // if (_itemPlacement == null || _itemPlacement.Collider2D == null || !collision.IsTouching(_itemPlacement.Collider2D)) return;
        if (_host != null && collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            InventoryManager.Instance.OnPowderChanged -= PowderChanged;
            host.RemoveInteractable(this);
            _host = null;
        }
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnPowderChanged -= PowderChanged;
    }

    private void PowderChanged()
    {
        if (InventoryManager.Instance.CanThrowPowder())
        {
            _host?.AddInteractable(this);
        }
        else
        {
            _host?.RemoveInteractable(this);
        }
    }
}
