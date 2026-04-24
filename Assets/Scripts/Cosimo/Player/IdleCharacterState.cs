
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleCharacterState : IStateCollision2D
{
    private Player _owner { get; }
    private PlayerController _ownerController;

    public IdleCharacterState(Player player, PlayerController controller)
    {
        _owner = player;
        _ownerController = controller;
    }
    public void OnEnd()
    {
       
    }

    public void OnFixedUpdate()
    {
        if(_ownerController.MoveDirection.sqrMagnitude >0.01f)
        {
            _owner.SetState(ECharacterStates.Walk);
        }
    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;
        _owner.Animator.SetBool("IsMoving",false);
        _owner.Animator.SetBool("IsAlive", true);
        _owner.Animator.SetBool("IsPlacing", false);
        _ownerController.InputActions.Player.Enable();
        _ownerController.ResetMoveDirection();

        Vector2 look = _ownerController.LastLookDirection;
        _owner.Animator.SetFloat("MoveX",look.x);
        _owner.Animator.SetFloat("MoveY",look.y);        
    }

    

    public void OnUpdate()
    {
       
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

    public void OnTriggerEnter2D(Collider2D collider)
    {
       
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        
    }
}