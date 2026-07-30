using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public static class InputUserEventsManager
{
    [Tooltip("The default input action asset assigned to InputUsers, it can be changed later at any time")]
    private static Type _defaultInputActionAsset = typeof(InputSystem_Actions);
    private static Dictionary<InputDevice, InputUser> _devicesUsers = new();

    public static Dictionary<InputDevice, InputUser> DeviceUsers => _devicesUsers;

    #region Public InputUser Events
    public static event Action<InputUser, InputDevice> OnUserAdded;
    public static event Action<InputUser, InputDevice> OnUserRemoved;
    public static event Action<InputUser, InputDevice> OnDevicePaired;
    public static event Action<InputUser, InputDevice> OnDeviceUnpaired;
    public static event Action<InputUser, InputDevice> OnDeviceLost;
    public static event Action<InputUser, InputDevice> OnDeviceRegained;
    public static event Action<InputUser, InputDevice> OnAccountChanged;
    public static event Action<InputUser, InputDevice> OnAccountNameChanged;
    public static event Action<InputUser, InputDevice> OnAccountSelectionInProgress;
    public static event Action<InputUser, InputDevice> OnAccountSelectionCanceled;
    public static event Action<InputUser, InputDevice> OnAccountSelectionComplete;
    public static event Action<InputUser, InputDevice> OnControlSchemeChanged;
    public static event Action<InputUser, InputDevice> OnControlsChanged;
    public static event Action<InputUser, InputDevice> OnPossibleHotSwap;
    public static event Action<InputDevice> OnNewDevice;
    #endregion

    public static InputUser? Player1
    {
        get
        {
            if (InputUser.all.Count > 0)
                return InputUser.all[0];
            else
                return null;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // remove all users if any
        while (InputUser.all.Count > 0)
        {
            InputUser user = InputUser.all[0];
            user.UnpairDevicesAndRemoveUser();
        }

        _devicesUsers.Clear();

        // Wipe all standard InputUserChange events
        OnUserAdded = null;
        OnUserRemoved = null;
        OnDevicePaired = null;
        OnDeviceUnpaired = null;
        OnDeviceLost = null;
        OnDeviceRegained = null;
        OnAccountChanged = null;
        OnAccountNameChanged = null;
        OnAccountSelectionInProgress = null;
        OnAccountSelectionCanceled = null;
        OnAccountSelectionComplete = null;
        OnControlSchemeChanged = null;
        OnControlsChanged = null;

        // Wipe your custom convenience events
        OnPossibleHotSwap = null;
        OnNewDevice = null;

        ++InputUser.listenForUnpairedDeviceActivity;

        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        InputUser.onChange -= OnInputChange;
        InputUser.onChange += OnInputChange;
        Application.quitting -= Cleanup;
        Application.quitting += Cleanup;
    }

    public static void Cleanup()
    {
        // remove all users if any
        while (InputUser.all.Count > 0)
        {
            InputUser user = InputUser.all[0];
            user.UnpairDevicesAndRemoveUser();
        }

        _devicesUsers.Clear();

        // Wipe all standard InputUserChange events
        OnUserAdded = null;
        OnUserRemoved = null;
        OnDevicePaired = null;
        OnDeviceUnpaired = null;
        OnDeviceLost = null;
        OnDeviceRegained = null;
        OnAccountChanged = null;
        OnAccountNameChanged = null;
        OnAccountSelectionInProgress = null;
        OnAccountSelectionCanceled = null;
        OnAccountSelectionComplete = null;
        OnControlSchemeChanged = null;
        OnControlsChanged = null;

        // Wipe your custom convenience events
        OnPossibleHotSwap = null;
        OnNewDevice = null;

        --InputUser.listenForUnpairedDeviceActivity;

        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
        InputUser.onChange -= OnInputChange;
        Application.quitting -= Cleanup;
    }

    private static void OnInputChange(InputUser user, InputUserChange change, InputDevice device)
    {
        switch (change)
        {
            case InputUserChange.Added:
                OnUserAdded?.Invoke(user, device);
                break;

            case InputUserChange.Removed:
                // Clean up devices from dictionary
                List<InputDevice> devicesToRemove = new();
                foreach (var item in _devicesUsers)
                {
                    if (item.Value == user)
                    {
                        devicesToRemove.Add(item.Key);
                    }
                }

                for (int i = 0; i < devicesToRemove.Count; i++)
                {
                    _devicesUsers.Remove(devicesToRemove[i]);
                }

                OnUserRemoved?.Invoke(user, device);
                break;

            case InputUserChange.DevicePaired:
                OnDevicePaired?.Invoke(user, device);
                break;

            case InputUserChange.DeviceUnpaired:
                OnDeviceUnpaired?.Invoke(user, device);
                break;

            case InputUserChange.DeviceLost:
                OnDeviceLost?.Invoke(user, device);
                break;

            case InputUserChange.DeviceRegained:
                OnDeviceRegained?.Invoke(user, device);
                break;

            case InputUserChange.AccountChanged:
                OnAccountChanged?.Invoke(user, device);
                break;

            case InputUserChange.AccountNameChanged:
                OnAccountNameChanged?.Invoke(user, device);
                break;

            case InputUserChange.AccountSelectionInProgress:
                OnAccountSelectionInProgress?.Invoke(user, device);
                break;

            case InputUserChange.AccountSelectionCanceled:
                OnAccountSelectionCanceled?.Invoke(user, device);
                break;

            case InputUserChange.AccountSelectionComplete:
                OnAccountSelectionComplete?.Invoke(user, device);
                break;

            case InputUserChange.ControlSchemeChanged:
                OnControlSchemeChanged?.Invoke(user, device);
                break;

            case InputUserChange.ControlsChanged:
                OnControlsChanged?.Invoke(user, device);
                break;
        }
    }

    private static void OnUnpairedDeviceUsed(InputControl control, InputEventPtr ptr)
    {
        InputDevice inputDevice = control.device;

        // InputControlScheme.FindControlSchemeForDevice(inputDevice, inputActions.controlSchemes);
        // having the IInputActionCollection2 we can get all the control schemes and by using InputControlScheme.FindControlSchemeForDevice
        // so having the device we can get its control scheme and thus update the UI for the buttons

        switch (inputDevice)
        {
            case Gamepad:
                break;
            case Keyboard:
                break;
            case Mouse:
                return;
            default:
                Debug.LogWarning(inputDevice.name + " is not a valid device");
                return;
        }

        InputUser inputUser;

        // device is new, assign it to the new user
        if (!_devicesUsers.ContainsKey(inputDevice))
        {
            OnNewDevice(inputDevice);
            return;
            //inputUser = InputUser.CreateUserWithoutPairedDevices();
            // inputUser = Player1.Value;
            // inputUser.UnpairDevices();

            // _devicesUsers[inputDevice] = inputUser;
            // Debug.Log($"Paired new device {inputDevice.name} to new user:{inputUser}");
        }

        // device has been used before by an user
        // HOTSWAP

        inputUser = _devicesUsers[inputDevice];
        // inputUser.UnpairDevices();
        OnPossibleHotSwap?.Invoke(inputUser, inputDevice);
        // Debug.Log($"Paired device {inputDevice.name} back to user:{inputUser}");
        // inputUser = InputUser.PerformPairingWithDevice(inputDevice, inputUser);

        // InputSystem_Actions inputActions = InputConfigManager.GetInputSytemInstanceGeneric<InputSystem_Actions>(inputUser);
        // if (inputUser.actions != inputActions)
        //     inputUser.AssociateActionsWithUser(inputActions);
        // switch (inputDevice)
        // {
        //     case Gamepad:
        //         inputUser.ActivateControlScheme(nameof(Gamepad));
        //         break;
        //     case Keyboard:
        //         inputUser.ActivateControlScheme("Keyboard&Mouse");
        //         break;
        // }
    }
}