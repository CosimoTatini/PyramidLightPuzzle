using UnityEngine;


public class Axe : MonoBehaviour
{
    [Header("Axe Settings")]
    [SerializeField] private float _rotationSpeed = 90f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;

        _rb.constraints = RigidbodyConstraints2D.FreezePosition;
    }

    private void FixedUpdate()
    {
        float nextAngle = _rb.rotation - (_rotationSpeed * Time.fixedDeltaTime);

        _rb.MoveRotation(nextAngle);
    }
}