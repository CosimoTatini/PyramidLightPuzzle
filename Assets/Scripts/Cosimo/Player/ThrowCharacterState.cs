using Assets.Scripts.Cosimo.Inventory;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

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
        if(InventoryManager.Instance.CanThrowPowder() && IsNearMagicTorch())
        {
            _animator.Play(_owner.ThrowSettings.clipName);
            InventoryManager.Instance.UsePowder();
            // Debug.Log($"Polvere lanciata di colore:{InventoryManager.Instance.SelectedPowder}");

        }

        else
        {
            // Debug.Log($"Polvere esaurita del colore:{InventoryManager.Instance.SelectedPowder}");
        }
       
    }

    private bool IsNearMagicTorch()
    {
        Vector3 interactionPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection * 0.8f;

        Vector3Int cellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);
        GameObject itemOnTile = PlacementManager.Instance.GetItemAt(cellPos);

        if (itemOnTile != null)
        {
            bool hasMagicalTorch = itemOnTile.GetComponentInChildren<MagicalTorch>() != null;

            if (hasMagicalTorch)
            {
                // Debug.Log($"[Throw] Rilevata una Torcia Magica sulla Tilemap nella cella {cellPos}. Lancio polvere consentito!");
                return true;
            }
        }

        // Debug.LogWarning($"[Throw] Nessuna torcia magica trovata sulla Tilemap nella cella {cellPos}. Lancio annullato.");
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