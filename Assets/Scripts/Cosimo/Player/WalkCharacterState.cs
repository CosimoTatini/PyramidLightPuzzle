using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class WalkCharacterState : IStateCollision2D
{
    private Player _owner { get; }
    private PlayerController _ownerController;
    public WalkCharacterState(Player player, PlayerController controller)
    {
        _owner = player;
        _ownerController = controller;
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
        Vector2 direction = _ownerController.MoveDirection;
        Vector2 targetVelocity = _ownerController.MoveDirection * _ownerController.MoveSpeed * Time.fixedDeltaTime;
        _ownerController.Rb.linearVelocity = targetVelocity + _ownerController.PlatformHandler.Velocity;

        if (direction.sqrMagnitude > 0.01f)
        {
            _owner.Animator.SetFloat("MoveX", direction.x);
            _owner.Animator.SetFloat("MoveY", direction.y);
        }
    }

    private void StopMove()
    {
        if(_ownerController.MoveDirection.sqrMagnitude<0.01f)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }

    public void OnStart()
    {
        _owner.Animator.SetBool("IsMoving",true);
        _owner.Animator.SetBool("IsAlive",true);
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
        StopMove();
    }
   
}