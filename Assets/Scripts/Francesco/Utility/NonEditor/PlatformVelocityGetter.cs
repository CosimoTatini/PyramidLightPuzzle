using UnityEngine;

public class PlatformVelocityGetter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;

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

    public Vector2? GetVelocity()
    {
        return _rb ? _rb.linearVelocity : null;
    }
}
