using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private List<InputUser> _activeInputUsers = new();

    void OnEnable()
    {
        ++InputUser.listenForUnpairedDeviceActivity;
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        InputUser.onChange += OnInputChange;
    }

    private void OnInputChange(InputUser user, InputUserChange change, InputDevice device)
    {
        switch(change)
        {
            case InputUserChange.Added:
            break;
            case InputUserChange.Removed:
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
            default:
                Debug.LogWarning(inputDevice.name + " is not a valid device");
                return;
        }

        InputUser inputUser = (_activeInputUsers.Count == 0) ? InputUser.CreateUserWithoutPairedDevices() : _activeInputUsers[0];
        inputUser = InputUser.PerformPairingWithDevice(inputDevice);
        InputSystem_Actions inputActions = InputConfigManager.Instance.GetInputSytemInstanceGeneric<InputSystem_Actions>(0);
        inputUser.AssociateActionsWithUser(inputActions);
        Debug.Log($"Paired new device {inputDevice.name}");
        switch (inputDevice)
        {
            case Gamepad:
                inputUser.ActivateControlScheme(nameof(Gamepad));
                break;
            case Keyboard:
                inputUser.ActivateControlScheme("Keyboard&Mouse");
                break;
        }

        _activeInputUsers.Add(inputUser);
    }
}
