
using UnityEngine;

/// <summary>
/// Axe Obstacle behaviour.
/// </summary>
public class Axe : MonoBehaviour
{
    
    [SerializeField] private float _speed = 90f;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _rb.angularVelocity = _speed;
    }


}

