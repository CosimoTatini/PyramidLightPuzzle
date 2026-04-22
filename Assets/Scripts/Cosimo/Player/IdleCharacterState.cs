
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleCharacterState : IState,IStateCollision2D
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