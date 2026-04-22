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

    public Rigidbody2D Rb => _rb;
    public InputSystem_Actions InputActions => _inputActions;
    public Vector2 MoveDirection => _moveDirection;
    public float MoveSpeed => _moveSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inputActions = new InputSystem_Actions();

        // Registriamo gli eventi una volta sola qui! 
        _inputActions.Player.Move.performed += ctx => _moveDirection = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += ctx => _moveDirection = Vector2.zero;
    }


    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

}
