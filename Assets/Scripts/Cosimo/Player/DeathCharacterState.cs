using System;
using System.Collections;
using UnityEngine;

public class DeathCharacterState : IState, IStateCollision2D
{

    private Player _owner;
    private PlayerController _ownerController;
    private bool _resetToFirst;
    private float _deathDuration = 1.5f;
    private float _timer;

    public DeathCharacterState(Player player,PlayerController controller)
    {
        _owner = player;
        _ownerController = controller;
    }

    public void SetUpDeath(bool resetToFirst)
    {
        _resetToFirst = resetToFirst;
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

        _timer = 0;

        Debug.Log("Animazione di morte playata");
    }

    public void OnUpdate()
    {
        _timer += Time.deltaTime;

        if(_timer>=_deathDuration)
        {
            if(_resetToFirst)
            {
                _owner.RespawnToFirst();
            }

            else
            {
                _owner.Respawn();
            }
        }
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