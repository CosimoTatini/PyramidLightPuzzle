using UnityEngine;

public class DeathCharacterState : IState
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
        throw new System.NotImplementedException();
    }

    public void OnFixedUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;

        _ownerController.InputActions.Player.Disable();

        //_owner.Animator.Play()
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}