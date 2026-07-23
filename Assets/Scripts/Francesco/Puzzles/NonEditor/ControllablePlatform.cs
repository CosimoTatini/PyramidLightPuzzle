using UnityEngine;

public class ControllablePlatform : MonoBehaviour, IVelocityProvider
{
    //NOTE, Can't be a ILightTriggerReceiver because there are multiple triggers working with it and putting this as a receiver
    // would make it hard to know who's calling the OnLightActivated method and would require to hard reference the 4 LightTriggers and 
    // do different stuff based on which one is calling, it's just easier to simply use the UnityEvents provided directly and call
    // MoveUp, MoveDown, MoveRight, MoveLeft from there
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float _movingSpeed = 3f;

    public Vector2 Velocity { get; private set; }

    public void Stop()
    {
        if (_rb == null) return;
        _rb.linearVelocity = Vector2.zero;
        Debug.Log("STOP");
    }

    public void MoveLeft()
    {
        if (_rb == null) return;
        // _rb.linearVelocity = Vector2.zero;
        _rb.linearVelocityX = -_movingSpeed;
    }

    public void MoveRight()
    {
        if (_rb == null) return;
        // _rb.linearVelocity = Vector2.zero;
        _rb.linearVelocityX = _movingSpeed;
    }

    public void MoveUp()
    {
        if (_rb == null) return;
        // _rb.linearVelocity = Vector2.zero;
        _rb.linearVelocityY = _movingSpeed;
    }

    public void MoveDown()
    {
        if (_rb == null) return;
        // _rb.linearVelocity = Vector2.zero;
        _rb.linearVelocityY = -_movingSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Stop();
    }

    void FixedUpdate()
    {
        if (_rb == null) Velocity = Vector2.zero;
        Velocity = _rb.linearVelocity;
    }
}