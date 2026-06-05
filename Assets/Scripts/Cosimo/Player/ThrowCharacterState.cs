using Assets.Scripts.Cosimo.Inventory;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEditor.Experimental.GraphView.GraphView;
/// <summary>
/// Handles the Throw State. Now this state is doing nothing . 
/// </summary>
internal class ThrowCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Animator _animator;
    private float _timer;
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

        
        if (!InventoryManager.Instance.CanThrowPowder()) 
        {
            Debug.LogWarning("[Throw] Impossibile lanciare: polvere esaurita nell'inventario.");
            _owner.SetState(ECharacterStates.Idle); 
            return; 
        }

       
        if (!IsNearMagicTorch(out LightEmitter targetEmitter))
        {
            Debug.LogWarning("[Throw] Impossibile lanciare: nessuna torcia magica rilevata davanti al giocatore.");
            _owner.SetState(ECharacterStates.Idle); 
            return;
        }

       
        PowderColor selectedColor = InventoryManager.Instance.SelectedPowder; 

        
        if (HasRoomForPowder(targetEmitter, selectedColor))
        {
            
            _animator.Play(_owner.ThrowSettings.clipName);
            InventoryManager.Instance.UsePowder(); 
            ApplyPowderToEmitter(targetEmitter, selectedColor);

            Debug.Log($"[Throw] Lancio completato con successo per il colore: {selectedColor}");
        }
        else
        {
            Debug.LogWarning($"[Throw] Il canale {selectedColor} della torcia è già pieno. Lancio annullato.");
            _owner.SetState(ECharacterStates.Idle); 
        }

    }

    private void ApplyPowderToEmitter(LightEmitter emitter, PowderColor selectedColor)
    {
        switch(selectedColor)
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

    private bool IsNearMagicTorch(out LightEmitter lightEmitter)
    {
        lightEmitter = null; // Inizializzazione di sicurezza
        Vector3 interactionPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection * 0.8f; //
        Vector3Int cellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos); //
        GameObject itemOnTile = PlacementManager.Instance.GetItemAt(cellPos); //

        if (itemOnTile != null) //
        {
            // 1. Cerchiamo prima il componente MagicalTorch che sappiamo essere presente
            var torch = itemOnTile.GetComponentInChildren<MagicalTorch>();

            if (torch != null)
            {
                // 2. Cerchiamo il LightEmitter sullo stesso oggetto, includendo anche quelli disattivati
                lightEmitter = torch.GetComponent<LightEmitter>();

                if (lightEmitter == null)
                {
                    lightEmitter = torch.GetComponentInChildren<LightEmitter>(true);
                }

                // Se lo abbiamo trovato, il controllo è superato con successo!
                if (lightEmitter != null)
                {
                    return true;
                }
            }
        }

        return false;
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

        if(_timer>=_owner.ThrowSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}