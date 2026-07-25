using UnityEngine;

public class CustomMummy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlatformHandler _platformHandler;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private bool _startGoingLeft = true;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private LayerMask _voidMasK;
    [SerializeField] private LayerMask _platformMask;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _xLeftPosition;
    [SerializeField] private float _xRightPosition;

    [Header("Impostazioni Movimento")]
    [SerializeField] private float _speedThreshold = 0.05f;

    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");

    private Vector2 _currentActiveDirection = Vector2.right;

    private bool _goingLeft;

    void Start()
    {
        if (_startGoingLeft)
            MoveLeft();
        else
            MoveRight();
    }

    private bool _wasFollowingPlatform = false;

    void FixedUpdate()
    {
        if (IsTouchingPlatform() && _platformHandler.IsMoving())
        {
            _wasFollowingPlatform = true;
            _rb.linearVelocity = _platformHandler.Velocity;
            return;
        }

        // give a little of breath room, when platform goes back its velocity goes to 0 for 1 frame, this avoids
        // calling ShouldChangeDirection even if the platform is still moving
        // other solution would be to access the platform directly anc check its _isMoving bool field rather than _platformHandler.IsMoving()
        // but this would cause the code to be even more specific
        if (_wasFollowingPlatform)
        {
            _wasFollowingPlatform = false;
            return;
        }

        if (ShouldChangeDirection())
        {
            if (_goingLeft)
                MoveRight();
            else
                MoveLeft();
        }
        else
        {
            if (_goingLeft)
                MoveLeft();
            else
                MoveRight();
        }
    }

    private bool IsTouchingPlatform()
    {
        return _collider.IsTouchingLayers(_platformMask);
    }

    private bool IsTouchingVoid()
    {
        return _collider.IsTouchingLayers(_voidMasK);
    }
    private bool IsTouchingGround()
    {
        return _collider.IsTouchingLayers(_groundMask);
    }

    private bool ShouldChangeDirection()
    {
        float currentPos = _rb.position.x;
        if (_goingLeft)
        {
            if (currentPos <= _xLeftPosition)
            {
                return true;
            }
        }
        else
        {
            if (currentPos >= _xRightPosition)
            {
                return true;
            }
        }

        // Touched Void go back
        if (!IsTouchingPlatform() && !IsTouchingGround() && IsTouchingVoid())
        {
            return true;
        }

        return false;
    }

    private void MoveLeft()
    {
        _rb.linearVelocity = new(-_movementSpeed, 0f);
        _spriteRenderer.flipX = true;
        _goingLeft = true;
        _currentActiveDirection = Vector2.left;
        ApplyDirection(_currentActiveDirection);
    }

    private void MoveRight()
    {
        _rb.linearVelocity = new(_movementSpeed, 0f);
        _spriteRenderer.flipX = false;
        _goingLeft = false;
        _currentActiveDirection = Vector2.right;
        ApplyDirection(_currentActiveDirection);
    }

    private void ApplyDirection(Vector2 dir)
    {
        _animator.SetFloat(VelocityXHash, dir.x);
        _animator.SetFloat(VelocityYHash, dir.y);
    }
}
