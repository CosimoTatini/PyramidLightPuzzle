using System;
using System.Collections;
using UnityEngine;

public class DeathCharacterState : IState, IStateCollision2D
{

    private Player _owner;
    private PlayerController _ownerController;

    public DeathCharacterState(Player player,PlayerController controller)
    {
        _owner = player;
        _ownerController = controller;
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

        _ownerController.InputActions.Player.Disable();

        _owner.Animator.SetBool("IsAlive",false);

        Debug.Log("Animazione di morte playata");
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
        if(collider.TryGetComponent<MummyObstacle>(out MummyObstacle mummy))
        {
            _owner.RespawnToFirst();
        }

        else if(collider.TryGetComponent<Obstacle>(out Obstacle obstacle))
        {
            _owner.Respawn();
        }

        _owner.SetState(ECharacterStates.Idle);
        _ownerController.InputActions.Player.Enable();
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
       
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
       
    }
}