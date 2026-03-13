using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class 
/// </summary>
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    [SerializeField] float _moveSpeed;
    private Rigidbody2D _rb;
    private Vector2 _moveDirection;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
    }



    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveDirection = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = _moveDirection * _moveSpeed * Time.fixedDeltaTime;

        _rb.linearVelocity = targetVelocity;
    }

}
