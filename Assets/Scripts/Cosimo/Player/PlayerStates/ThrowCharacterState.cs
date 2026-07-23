using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
/// <summary>
/// Handles the Throw State. When you throw, based on the color does +1 on the R,G,B value
/// </summary>
internal class ThrowCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Animator _animator;
    private float _timer;

    private LightEmitter lightEmitter;
    public ThrowCharacterState(Player player, PlayerController playerController, Animator animator)
    {
        _owner = player;
        _ownerController = playerController;
        _animator = animator;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
    }


    public void OnCollisionExit2D(Collision2D collision)
    {

    }

    public void OnCollisionStay2D(Collision2D collision)
    {

    }

    public void OnEnd()
    {

    }


    public void OnFixedUpdate()
    {

    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;
        _timer = 0f;
        _animator.Play(_owner.ThrowSettings.clipName);

        // if (!InventoryManager.Instance.CanThrowPowder())
        // {
        //     Debug.LogWarning("[Throw] Impossibile lanciare: polvere esaurita nell'inventario.");
        //     _owner.SetState(ECharacterStates.Idle);
        //     return;
        // }


        // if (!IsOverTorch(out LightEmitter targetEmitter))
        // {
        //     Debug.LogWarning("[Throw] Impossibile lanciare: nessuna torcia magica rilevata davanti al giocatore.");
        //     _owner.SetState(ECharacterStates.Idle);
        //     return;
        // }


        // PowderColor selectedColor = InventoryManager.Instance.SelectedPowder;


        // if (HasRoomForPowder(targetEmitter, selectedColor))
        // {

        //     _animator.Play(_owner.ThrowSettings.clipName);
        //     InventoryManager.Instance.UsePowder();
        //     ApplyPowderToEmitter(targetEmitter, selectedColor);

        //     Debug.Log($"[Throw] Lancio completato con successo per il colore: {selectedColor}");
        // }
        // else
        // {
        //     Debug.LogWarning($"[Throw] Il canale {selectedColor} della torcia è già pieno. Lancio annullato.");
        //     _owner.SetState(ECharacterStates.Idle);
        // }

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

    private bool IsOverTorch(out LightEmitter lightEmitter)
    {
        lightEmitter = null;
        GameObject magicalTorch = PlacementManager.Instance.FindItemOfType(typeof(MagicalTorch));
        if (magicalTorch == null || !magicalTorch.TryGetComponent(out ItemPlacement itemPlacement))
        {
            return false;
        }
        if (!_owner.PlayerController.Rb.IsTouching(itemPlacement.Collider2D))
        {
            return false;
        }

        if (magicalTorch.TryGetComponent(out lightEmitter))
        {
            return true;
        }
        else
        {
            return false;
        }
        //     Vector3 interactionPos = _owner.transform.position;
        // Vector3Int cellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);
        // GameObject itemOnTile = PlacementManager.Instance.GetItemAt(cellPos, PlacementManager.Instance.TargetTilemap);

        // if (itemOnTile != null)
        // {
        //     var torch = itemOnTile.GetComponentInChildren<MagicalTorch>();

        //     if (torch != null)
        //     {
        //         lightEmitter = torch.GetComponent<LightEmitter>();

        //         if (lightEmitter == null)
        //         {
        //             lightEmitter = torch.GetComponentInChildren<LightEmitter>(true);
        //         }
        //         if (lightEmitter != null)
        //         {
        //             return true;
        //         }
        //     }
        // }

        // return false;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {

    }

    public void OnTriggerExit2D(Collider2D collider)
    {

    }

    public void OnTriggerStay2D(Collider2D collider)
    {

    }

    public void OnUpdate()
    {
        _timer += Time.deltaTime;

        if (_timer >= _owner.ThrowSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}