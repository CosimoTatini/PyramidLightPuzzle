
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleCharacterState : IState
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
        throw new System.NotImplementedException();
    }

    public void OnFixedUpdate()
    {
        Vector2 moveInput = _ownerController.MoveDirection;

       
        if (moveInput.sqrMagnitude > 0.01f)
        {
           _owner.SetState(ECharacterStates.Walk);
        }
    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;
        _owner.Animator.Play("Idle");
        
    }

    private void Move(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}