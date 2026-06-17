using DesignPatterns.Generics;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class PlayerInputManager : Singleton<PlayerInputManager>
{
    [SerializeField] private List<InputUser> _activeInputUsers = new();

    private Dictionary<InputDevice, InputUser> _devicesUsers = new();

    public InputUser? Player1
    {
        get
        {
            if (InputUser.all.Count > 0)
                return InputUser.all[0];
            else
                return null;
        }
    }

    private void Start()
    {
        while (InputUser.all.Count > 0)
        {
            InputUser user = InputUser.all[0];
            user.UnpairDevicesAndRemoveUser();
        }
        // pair player 1 to keyboard
        InputUser player1 = InputUser.CreateUserWithoutPairedDevices();
        InputDevice keyboard = Keyboard.current;
        InputUser.PerformPairingWithDevice(keyboard, player1);
        var inputActions = InputConfigManager.Instance.GetInputSytemInstanceGeneric<InputSystem_Actions>(player1);
        player1.AssociateActionsWithUser(inputActions);
        player1.ActivateControlScheme("Keyboard&Mouse");

        _devicesUsers[keyboard] = player1;
    }

    void OnEnable()
    {
        ++InputUser.listenForUnpairedDeviceActivity;
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        InputUser.onChange += OnInputChange;
    }

    private void OnInputChange(InputUser user, InputUserChange change, InputDevice device)
    {
        switch (change)
        {
            case InputUserChange.Added:
                break;
            case InputUserChange.Removed:

                List<InputDevice> devicesToRemove = new();
                // clean up devices from dictionary
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

                break;
            case InputUserChange.DevicePaired:
                break;
            case InputUserChange.DeviceUnpaired:
                break;
            case InputUserChange.DeviceLost:
                break;
            case InputUserChange.DeviceRegained:
                break;
            case InputUserChange.AccountChanged:
                break;
            case InputUserChange.AccountNameChanged:
                break;
            case InputUserChange.AccountSelectionInProgress:
                break;
            case InputUserChange.AccountSelectionCanceled:
                break;
            case InputUserChange.AccountSelectionComplete:
                break;
            case InputUserChange.ControlSchemeChanged:
                
                Debug.Log("Control scheme changed " + user.id + " " + device?.name);
                break;
            case InputUserChange.ControlsChanged:
                break;
        }
    }

    void OnDisable()
    {
        --InputUser.listenForUnpairedDeviceActivity;
        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
        InputUser.onChange -= OnInputChange;
    }

    private void OnUnpairedDeviceUsed(InputControl control, InputEventPtr ptr)
    {
        InputDevice inputDevice = control.device;

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

        // device has been used before by an user
        if (_devicesUsers.ContainsKey(inputDevice))
        {
            inputUser = _devicesUsers[inputDevice];
            inputUser.UnpairDevices();
            Debug.Log($"Paired device {inputDevice.name} back to user:{inputUser}");
        }
        else
        // device is new, assign it to the new user
        {
            //inputUser = InputUser.CreateUserWithoutPairedDevices();
            inputUser = Player1.Value;
            inputUser.UnpairDevices();

            _devicesUsers[inputDevice] = inputUser;
            Debug.Log($"Paired new device {inputDevice.name} to new user:{inputUser}");
        }

        inputUser = InputUser.PerformPairingWithDevice(inputDevice, inputUser);

        InputSystem_Actions inputActions = InputConfigManager.Instance.GetInputSytemInstanceGeneric<InputSystem_Actions>(inputUser);
        if(inputUser.actions != inputActions)
        inputUser.AssociateActionsWithUser(inputActions);
        switch (inputDevice)
        {
            case Gamepad:
                inputUser.ActivateControlScheme(nameof(Gamepad));
                break;
            case Keyboard:
                inputUser.ActivateControlScheme("Keyboard&Mouse");
                
                break;
        }
    }
}