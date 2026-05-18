using UnityEngine;

public class PlatformVelocityGetter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private bool _usePosition;
    [SerializeField] private MovingPlatform _useRotation;

    private Vector2 _lastPosition;
    private Vector2 _velocity;

    private void Awake()
    {
        if (!_rb)
        {
            if (!TryGetComponent(out _rb))
            {
                Debug.LogWarning($"{name} doesn't have a Rigidbody");
            }
        }
    }

    void FixedUpdate()
    {
        if (_usePosition)
        {
            _velocity = _useRotation.Velocity;
            _lastPosition = _rb.position;
        }
    }

    public Vector2? GetVelocity()
    {
        if (_usePosition)
        {
            return _velocity;
        }
        return _rb ? _rb.linearVelocity : null;
    }
}
