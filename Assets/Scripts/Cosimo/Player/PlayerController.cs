using System;
using UnityEditorInternal;
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
    private Vector2 _lastLookDirection = Vector2.down;


    public Rigidbody2D Rb => _rb;
    public InputSystem_Actions InputActions => _inputActions;
    public Vector2 MoveDirection => _moveDirection;
    public float MoveSpeed => _moveSpeed;

    public Vector2 LastLookDirection => _lastLookDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inputActions = new InputSystem_Actions();

        _inputActions.Player.Move.performed += ctx => {
            _moveDirection = ctx.ReadValue<Vector2>();
            if (_moveDirection.sqrMagnitude > 0.01f)
                _lastLookDirection = _moveDirection.normalized; // Salviamo la direzione
        };
        _inputActions.Player.Move.canceled += ctx => _moveDirection = Vector2.zero;
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Switch.performed += OnSwitchItem;
    }

    private void OnSwitchItem(InputAction.CallbackContext context)
    {
        GetComponent<Player>().HandleSwitch();
    }

    private void OnInteract(InputAction.CallbackContext obj)
    {
        GetComponent<Player>().HandleInteract();
    }

    public void ResetMoveDirection()
    {
        _moveDirection = Vector2.zero;
    }
    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

}
