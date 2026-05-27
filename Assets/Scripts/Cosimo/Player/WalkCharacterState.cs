using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class WalkCharacterState : IStateCollision2D
{
    private Player _owner { get; }
    private PlayerController _ownerController;
    private Animator _animator;
    public WalkCharacterState(Player player, PlayerController controller,Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _animator= animator;
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
        _owner.Animator.Play(_owner.WalkSettings.clipName);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.TryGetComponent<PowderColorChooser>(out var powder))
        {
            _owner.DetectedObject=collider.gameObject;
            _owner.SetState(ECharacterStates.Grab);
        }
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