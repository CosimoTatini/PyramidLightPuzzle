using Assets.Scripts.Cosimo.Inventory;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

internal class ThrowCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Animator _animator;
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
        if(InventoryManager.Instance.CanThrowPowder() && IsNearMagicTorch())
        {
            _animator.Play(_owner.ThrowSettings.clipName);
            InventoryManager.Instance.UsePowder();
            Debug.Log($"Polvere lanciata di colore:{InventoryManager.Instance.SelectedPowder}");

        }

        else
        {
            Debug.Log($"Polvere esaurita del colore:{InventoryManager.Instance.SelectedPowder}");
        }
        _owner.SetState(ECharacterStates.Idle);
    }

    private bool IsNearMagicTorch()
    {
        Vector2 checkPos = (Vector2)_owner.transform.position + _ownerController.LastLookDirection * 0.8f;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.3f);
        if (hit != null)
        {
            Debug.Log($"[Fisica] Cerchio di controllo ha colpito: {hit.name}");
        }
        return hit != null && hit.TryGetComponent<MagicalTorch>(out _);
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
        
    }
}