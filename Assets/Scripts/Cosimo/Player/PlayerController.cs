using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class that handles the player physics and the Inputsystem Actions.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputConfigSO _playerConfig;
    public InputConfigSO PlayerConfig => _playerConfig;
    private InputSystem_Actions _inputActions;
    [SerializeField] float _moveSpeed;
    private Rigidbody2D _rb;
    private Vector2 _moveDirection;
    private Vector2 _lastLookDirection = Vector2.down;
    private Collider2D _collider;

    [SerializeField] private PlatformHandler _platformHandler;

    [SerializeField] private CameraZoomController _cameraController;

    public PlatformHandler PlatformHandler => _platformHandler;
    public Rigidbody2D Rb => _rb;
    public InputSystem_Actions InputActions => _inputActions;
    public Vector2 MoveDirection => _moveDirection;
    public float MoveSpeed => _moveSpeed;

    public Vector2 LastLookDirection => _lastLookDirection;

    public Collider2D Collider => _collider;




    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (!InputEventsManager.Player1.HasValue)
        {
            Debug.LogWarning("Can't get InputActions, No players detected");
            return;
        }
        _inputActions = InputConfigManager.GetInputSytemInstanceGeneric<InputSystem_Actions>(InputEventsManager.Player1.Value);
        //    _inputActions.Enable();

        _inputActions.Player.Move.performed += ctx =>
        {
            _moveDirection = ctx.ReadValue<Vector2>();
            if (_moveDirection.sqrMagnitude > 0.01f)
                _lastLookDirection = _moveDirection.normalized;
        };
        _inputActions.Player.Move.canceled += ctx => _moveDirection = Vector2.zero;
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Switch.performed += OnSwitchItem;
        _inputActions.Player.SetRed.performed += _ => InventoryManager.Instance.SelectPowder(PowderColor.Red);
        _inputActions.Player.SetGreen.performed += _ => InventoryManager.Instance.SelectPowder(PowderColor.Green);
        _inputActions.Player.SetBlue.performed += _ => InventoryManager.Instance.SelectPowder(PowderColor.Blue);
        _inputActions.Player.NextColor.performed += _ => InventoryManager.Instance.CyclePowder(1);
        _inputActions.Player.PreviousColor.performed += _ => InventoryManager.Instance.CyclePowder(-1);
        _inputActions.Player.Throw.performed += _ => GetComponent<Player>().HandleThrow();
        _inputActions.Player.ZoomIn.performed += _ =>
        {
            if (_cameraController != null)
            {
                _cameraController.ZoomIn();
            }
        };

        _inputActions.Player.ZoomOut.performed += _ =>
        {
            if (_cameraController != null)
            {
                _cameraController.ZoomOut();
            }
        };


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

    public void EnableInput()
    {
        _inputActions?.Player.Enable();
    }

    public void DisableInput()
    {
        _inputActions?.Player.Disable();
        _moveDirection= Vector2.zero;
    }
    // private void OnEnable() => _inputActions.Enable();
    // private void OnDisable() => _inputActions.Disable();

}
