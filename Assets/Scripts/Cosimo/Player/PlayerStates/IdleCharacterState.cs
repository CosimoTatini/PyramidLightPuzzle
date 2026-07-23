
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the Idle State.
/// </summary>
public class IdleCharacterState : IStateCollision2D
{
    private Player _owner { get; }
    private PlayerController _ownerController;
    private Animator _animator;

    public IdleCharacterState(Player player, PlayerController controller, Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _animator = animator;
    }
    public void OnEnd()
    {

    }

    public void OnFixedUpdate()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero + _ownerController.PlatformHandler.Velocity;
        if (_ownerController.MoveDirection.sqrMagnitude > 0.01f)
        {
            _owner.SetState(ECharacterStates.Walk);
        }
    }

    public void OnStart()
    {
        InputConfigManager.RegisterConfig(_ownerController.PlayerConfig);
        if (_ownerController.MoveDirection.sqrMagnitude > 0.01f)
        {
            _owner.SetState(ECharacterStates.Walk);
            return;
        }
        _ownerController.Rb.linearVelocity = Vector2.zero + _ownerController.PlatformHandler.Velocity;
        //_owner.Animator.SetBool("IsMoving",false);
        //_owner.Animator.SetBool("IsAlive", true);
        //_owner.Animator.SetBool("IsPlacing", false);
        _owner.Animator.Play(_owner.IdleSettings.clipName);
        // _ownerController.InputActions.Player.Enable();
        _ownerController.ResetMoveDirection();

        Vector2 look = _ownerController.LastLookDirection;
        _owner.Animator.SetFloat("MoveX", look.x);
        _owner.Animator.SetFloat("MoveY", look.y);
    }



    public void OnUpdate()
    {
        _owner.CalculateCurrentInteractables();
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