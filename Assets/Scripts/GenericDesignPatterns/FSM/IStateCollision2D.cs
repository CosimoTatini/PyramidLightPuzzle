using UnityEngine;

/// <summary>
/// An IState handling 2D collisions and triggers
/// </summary>
public interface IStateCollision2D : IState
{
    public void OnCollisionEnter2D(Collision2D collision);
    public void OnCollisionExit2D(Collision2D collision);
    public void OnCollisionStay2D(Collision2D collision);
    public void OnTriggerEnter2D(Collider2D collider);
    public void OnTriggerStay2D(Collider2D collider);
    public void OnTriggerExit2D(Collider2D collider);
}