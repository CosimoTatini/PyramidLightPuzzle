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

    // Public properties
    public PlatformHandler PlatformHandler => _platformHandler;
    public Rigidbody2D Rb => _rb;
    public InputSystem_Actions InputActions => _inputActions;
    public Vector2 MoveDirection => _moveDirection;
    public float MoveSpeed => _moveSpeed;
    public Vector2 LastLookDirection => _lastLookDirection;
    public Collider2D Collider => _collider;

    private void Awake()
    {
        // Get required components
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // Validate and get Input Actions
        if (!InputUserEventsManager.Player1.HasValue)
        {
            Debug.LogWarning("Can't get InputActions, No players detected");
            return;
        }

        _inputActions = InputConfigManager.GetInputSytemInstanceGeneric<InputSystem_Actions>(InputUserEventsManager.Player1.Value);
    }

    private void OnEnable()
    {
        if (_inputActions == null) return;

        // Subscribe to all input events using NAMED methods (no lambdas!)
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Switch.performed += OnSwitchItem;
        _inputActions.Player.SetRed.performed += OnSetRed;
        _inputActions.Player.SetGreen.performed += OnSetGreen;
        _inputActions.Player.SetBlue.performed += OnSetBlue;
        _inputActions.Player.NextColor.performed += OnNextColor;
        _inputActions.Player.PreviousColor.performed += OnPreviousColor;
        _inputActions.Player.Throw.performed += OnThrow;
        _inputActions.Player.ZoomIn.performed += OnZoomIn;
        _inputActions.Player.ZoomOut.performed += OnZoomOut;
    }

    private void OnDisable()
    {
        if (_inputActions == null) return;

        // UNSUBSCRIBE from EVERYTHING to prevent MissingReferenceException
        _inputActions.Player.Move.performed -= OnMovePerformed;
        _inputActions.Player.Move.canceled -= OnMoveCanceled;
        _inputActions.Player.Interact.performed -= OnInteract;
        _inputActions.Player.Switch.performed -= OnSwitchItem;
        _inputActions.Player.SetRed.performed -= OnSetRed;
        _inputActions.Player.SetGreen.performed -= OnSetGreen;
        _inputActions.Player.SetBlue.performed -= OnSetBlue;
        _inputActions.Player.NextColor.performed -= OnNextColor;
        _inputActions.Player.PreviousColor.performed -= OnPreviousColor;
        _inputActions.Player.Throw.performed -= OnThrow;
        _inputActions.Player.ZoomIn.performed -= OnZoomIn;
        _inputActions.Player.ZoomOut.performed -= OnZoomOut;

        // Reset movement to prevent lingering input
        _moveDirection = Vector2.zero;
    }

    // --- NAMED CALLBACK METHODS (replace all lambdas) ---

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _moveDirection = ctx.ReadValue<Vector2>();
        if (_moveDirection.sqrMagnitude > 0.01f)
            _lastLookDirection = _moveDirection.normalized;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _moveDirection = Vector2.zero;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        GetComponent<Player>().HandleInteract();
    }

    private void OnSwitchItem(InputAction.CallbackContext ctx)
    {
        GetComponent<Player>().HandleSwitch();
    }

    private void OnSetRed(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.SelectPowder(PowderColor.Red);
    }

    private void OnSetGreen(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.SelectPowder(PowderColor.Green);
    }

    private void OnSetBlue(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.SelectPowder(PowderColor.Blue);
    }

    private void OnNextColor(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.CyclePowder(1);
    }

    private void OnPreviousColor(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.CyclePowder(-1);
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        GetComponent<Player>().HandleThrow();
    }

    private void OnZoomIn(InputAction.CallbackContext ctx)
    {
        if (_cameraController != null)
        {
            _cameraController.ZoomIn();
        }
    }

    private void OnZoomOut(InputAction.CallbackContext ctx)
    {
        if (_cameraController != null)
        {
            _cameraController.ZoomOut();
        }
    }

    // --- PUBLIC METHODS (for external calls) ---

    public void ResetMoveDirection()
    {
        _moveDirection = Vector2.zero;
    }

    public void EnableInput()
    {
        InputConfigManager.RegisterConfig(_playerConfig);
        // Note: The actual InputActionMap is already enabled via OnEnable.
        // If you need to force-enable it here, uncomment the line below:
        // _inputActions?.Enable();
    }

    public void DisableInput()
    {
        InputConfigManager.UnregisterConfig(_playerConfig);
        _moveDirection = Vector2.zero;
        // The actual InputActionMap is disabled in OnDisable.
        // If you call this externally, you may want to also disable the map:
        // _inputActions?.Disable();
    }
}