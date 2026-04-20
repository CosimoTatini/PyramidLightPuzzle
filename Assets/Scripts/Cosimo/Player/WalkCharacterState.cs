using UnityEngine;

 public class WalkCharacterState : IState,IStateCollision2D
 {
    private Player _owner { get; }
    private PlayerController _controller;
    public WalkCharacterState(Player player,PlayerController controller)
    {
        _owner = player;
        _controller = controller;
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnFixedUpdate()
    {
        Vector2 moveDirection = _controller.InputActions.Player.Move.ReadValue<Vector2>();
        _controller.Rb.linearVelocity= moveDirection * _controller.MoveSpeed * Time.fixedDeltaTime;
    }

    public void OnStart()
    {
        _owner.Animator.Play("Walk");
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        if(_controller.InputActions.Player.Move.ReadValue<Vector2>().sqrMagnitude<0.01f)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}