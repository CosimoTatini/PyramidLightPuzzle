using System.Linq;
using DesignPatterns.Generics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using IEM = InputEventsManager;

// To display the icons I need to have a referenceto the inputUser, this lets me retrieve the control scheme
// then in the UI i can retrieve the list of all enabled inputActions (need to put a list of hidden ones, so they are skipped when creating the rows, for instance Movement)
// 

public class LobbyManager : Singleton<LobbyManager>
{
    [Tooltip("Immediately creates player1 on awake")]
    [SerializeField] private bool _initializeFirstPlayer = true;
    [Tooltip("Adjusts behavior based on single or local-multiplayer")]
    [SerializeField] private bool _singlePlayer = true;
    [SerializeField] private TypeVar _defaultInputActionAsset;

    protected override void Awake()
    {
        base.Awake();

        if (_initializeFirstPlayer)
        {
            // pair player 1 to keyboard
            InputDevice deviceToPair = null;

            string controlScheme = string.Empty;
            if (Gamepad.current != null)
            {
                controlScheme = nameof(Gamepad);
                deviceToPair = Gamepad.current;
            }
            else
            {
                controlScheme = "Keyboard&Mouse";
                deviceToPair = Keyboard.current;
            }

            InputUser inputUser = CreateNewUser();
            if (AssignDeviceToUser(deviceToPair, inputUser))
            {
                Debug.Log("Successfully created player1");
            }
            else
            {
                Debug.LogWarning("Something went wrong when creating player1");
            }

            if (deviceToPair != null)
            {
                // InputUser player1 = InputUser.CreateUserWithoutPairedDevices();
                // InputUser.PerformPairingWithDevice(deviceToPair, player1);
                // var inputActions = InputConfigManager.GetInputSystemInstance(player1, _defaultInputActionAsset.Type);
                // player1.AssociateActionsWithUser(inputActions);
                // player1.ActivateControlScheme(controlScheme);

                // IEM.DeviceUsers[deviceToPair] = player1;
            }
            else
            {
                Debug.LogError("No physical device detected");
            }
        }
    }

    void OnEnable()
    {
        IEM.OnNewDevice -= NewInputDeviceDetected;
        IEM.OnNewDevice += NewInputDeviceDetected;
        IEM.OnPossibleHotSwap -= HotSwapInputDevice;
        IEM.OnPossibleHotSwap += HotSwapInputDevice;
    }

    void OnDisable()
    {
        IEM.OnNewDevice -= NewInputDeviceDetected;
        IEM.OnPossibleHotSwap -= HotSwapInputDevice;
    }

    public bool ChangeUserActions(InputUser inputUser, IInputActionCollection2 newActions)
    {
        if (inputUser == null || !inputUser.valid) return false;

        inputUser.AssociateActionsWithUser(newActions);
        return true;
    }

    public bool ChangeDeviceOwnership(InputUser oldUser, InputUser newUser, InputDevice inputDevice)
    {
        if (inputDevice == null) return false;
        if (oldUser == null || !oldUser.valid) return false;
        if (newUser == null || !newUser.valid) return false;

        if (IEM.DeviceUsers.TryGetValue(inputDevice, out var inputUser))
        {
            // olduser actually has ownership
            if (inputUser == oldUser)
            {
                IEM.DeviceUsers.Remove(inputDevice);
                oldUser.UnpairDevice(inputDevice);
            }
            else
            {
                return false;
            }
        }

        return AssignDeviceToUser(inputDevice, newUser);
    }

    /// <summary>
    /// Assigns an unpaired device to an inputUser
    /// </summary>
    /// <param name="inputDevice"></param>
    /// <param name="inputUser"></param>
    /// <returns>True if successful, false otherwise</returns>
    public bool AssignDeviceToUser(InputDevice inputDevice, InputUser inputUser)
    {
        if (InputUser.FindUserPairedToDevice(inputDevice).HasValue) return false;
        if (inputUser == null || !inputUser.valid) return false;
        if (inputUser.pairedDevices.Contains(inputDevice)) return false;

        var inputControlScheme = GetInputControlScheme(inputUser, inputDevice);
        if (!inputControlScheme.HasValue) return false;

        IEM.DeviceUsers.Remove(inputDevice);
        inputUser.UnpairDevices();
        InputUser.PerformPairingWithDevice(inputDevice, inputUser);
        inputUser.ActivateControlScheme(inputControlScheme.Value);
        IEM.DeviceUsers.Add(inputDevice, inputUser);
        return true;
    }

    //TODO: for now we automatically pair every device to player1, this can go much deeper as there could be a choice menu so you can choose to assign the device
    // to other users and create new user too and then assign the device by pressing it again
    /// <summary>
    /// Handles new devices
    /// </summary>
    /// <param name="inputDevice"></param>
    public void NewInputDeviceDetected(InputDevice inputDevice)
    {
        if (_singlePlayer)
        {
            if (!IEM.Player1.HasValue)
            {
                CreateNewUser();
            }
            AssignDeviceToUser(inputDevice, IEM.Player1.Value);
        }
        else
        {

        }
    }

    private void HotSwapInputDevice(InputUser inputUser, InputDevice inputDevice)
    {
        AssignDeviceToUser(inputDevice, inputUser);
    }

    public InputUser CreateNewUser(IInputActionCollection2 inputActions = null)
    {
        InputUser newUser = InputUser.CreateUserWithoutPairedDevices();
        inputActions ??= InputConfigManager.GetInputSystemInstance(newUser, _defaultInputActionAsset.Type);
        newUser.AssociateActionsWithUser(inputActions);
        return newUser;
    }

    public InputControlScheme? GetInputControlScheme(InputUser inputUser, InputDevice inputDevice)
    {
        if (inputDevice == null) return null;
        if (inputUser == null || !inputUser.valid) return null;

        var actions = inputUser.actions;
        return InputControlScheme.FindControlSchemeForDevice(inputDevice, actions.controlSchemes);
    }
}