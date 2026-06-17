using System;
using System.Collections.Generic;
using System.Linq;
using DesignPatterns.Generics;
using UnityEngine;
using NativeSerializableDictionary;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputConfigManager : Singleton<InputConfigManager>
{
    private Dictionary<InputUser, Dictionary<string, List<InputActionStruct>>> _actionsStacks = new();
    private Dictionary<InputUser, Dictionary<string, InputAction>> _actionsCaches = new();
    private Dictionary<InputUser, List<InputAction>> _actionsEnabled = new();
    private Dictionary<InputUser, List<InputAction>> _actionsDisabled = new();
    private Dictionary<InputUser, InputBundle> _inputBundles = new();

    private Dictionary<InputUser, EnabledDisabledAction> _enabledDisabledActionsEvents = new();
    public struct EnabledDisabledAction
    {
        public Action OnEnabledActionsChanged;
        public Action OnDisabledActionsChanged;
    }

    private void RegisterAction(InputUser id, InputActionStruct actionData/*, Type inputAssetCsharp*/)
    {
        if (string.IsNullOrEmpty(actionData.Guid))
        {
            return;
        }

        if (!_actionsStacks.ContainsKey(id)) _actionsStacks[id] = new();
        var actionStackDict = _actionsStacks[id];

        if (!actionStackDict.ContainsKey(actionData.Guid)) actionStackDict[actionData.Guid] = new();
        var actionStack = actionStackDict[actionData.Guid];

        if (actionStack.Contains(actionData))
        {
            return;
        }

        actionStack.Add(actionData);


        // we added the only item
        if (actionStack.Count == 1)
        {
            InputAction inputAction = GetAction(id, actionStack[0].Guid);
            if (inputAction == null) return;

            if (actionData.Enabled == inputAction.enabled)
            {
                return;
            }
            else
            {
                if (inputAction.enabled)
                {
                    inputAction.Disable();
                    RemoveEnabledAction(id, inputAction);
                    AddDisabledAction(id, inputAction);
                }
                else
                {
                    inputAction.Enable();
                    RemoveDisabledAction(id, inputAction);
                    AddEnabledAction(id, inputAction);
                }
            }

            return;
        }

        // CACHE
        // CacheActionIfNotAlready(id, actionData.Guid, inputAssetCsharp);

        InputActionStruct firstElement = actionStack[0];

        // SORT
        actionStack.Sort((a, b) => b.Priority.CompareTo(a.Priority));

        // check if first element changed
        if (!actionStack[0].Equals(firstElement))
        {
            // if so, we need to check the state of it
            InputAction inputAction = GetAction(id, actionStack[0].Guid);
            if (inputAction != null)
            {
                // if action status is the same of previous one, there's nothing to do 
                if (actionStack[0].Enabled == inputAction.enabled) return;

                if (actionStack[0].Enabled)
                {
                    inputAction.Enable();
                    RemoveDisabledAction(id, inputAction);
                    AddEnabledAction(id, inputAction);
                }
                else
                {
                    inputAction.Disable();
                    RemoveEnabledAction(id, inputAction);
                    AddDisabledAction(id, inputAction);
                }
            }
        }
    }

    private void UnregisterAction(InputUser id, InputActionStruct actionData)
    {
        if (!_actionsStacks.ContainsKey(id)) return;
        var actionStackDict = _actionsStacks[id];

        if (!actionStackDict.ContainsKey(actionData.Guid)) return;
        var actionStack = actionStackDict[actionData.Guid];

        if (actionStack.Count == 0 || !actionStack.Contains(actionData)) return;

        InputActionStruct firstElement = actionStack[0];

        actionStack.Remove(actionData);

        InputAction inputAction = GetAction(id, actionData.Guid);
        // we removed the only element in the list, remove it from enabled/disabled list and also call O
        if (actionStack.Count == 0)
        {
            // if action was enabled, disable it and remove it from enabled actions and add it to disable actions
            if (inputAction.enabled)
            {
                inputAction.Disable();
                RemoveEnabledAction(id, inputAction);
                AddDisabledAction(id, inputAction);
            }
            else
            {

            }
            return;
        }

        // we removed first element, we need to process the new first element if there's one
        if (!firstElement.Equals(actionStack[0]))
        {
            if (inputAction == null) return;

            // if old and new first elements have different enabled values
            if (actionStack[0].Enabled == inputAction.enabled) return;

            if (actionStack[0].Enabled)
            {
                inputAction.Enable();
                RemoveDisabledAction(id, inputAction);
                AddEnabledAction(id, inputAction);
            }
            else
            {
                inputAction.Disable();
                RemoveEnabledAction(id, inputAction);
                AddDisabledAction(id, inputAction);
            }
        }
        else
        {

        }
    }

    private void RegisterActionMap(InputUser id, InputMapStruct mapData)
    {
        if (mapData.InputActionStructs.Count == 0) return;
        foreach (var actionData in mapData.InputActionStructs)
        {
            RegisterAction(id, actionData);
        }
    }

    private void UnregisterActionMap(InputUser id, InputMapStruct mapData)
    {
        if (mapData.InputActionStructs.Count == 0) return;
        foreach (var actionData in mapData.InputActionStructs)
        {
            UnregisterAction(id, actionData);
        }
    }

    public void RegisterConfig(InputConfigSO configSO, InputUser id)
    {
        var inputAssetMapLists = configSO.GetInputAssetMaps();
        foreach (var inputAssetMapList in inputAssetMapLists)
        {
            // skip invalid lists
            if (inputAssetMapList.AssetType == null || !typeof(IInputActionCollection2).IsAssignableFrom(inputAssetMapList.AssetType.Type)) continue;

            foreach (var inputMapStruct in inputAssetMapList.InputMapStructs)
            {
                RegisterActionMap(id, inputMapStruct);
            }
        }
    }

    /// <summary>
    /// Registers the passed config to Player1
    /// </summary>
    /// <param name="configSO"></param>
    public void RegisterConfig(InputConfigSO configSO)
    {
        if (PlayerInputManager.Instance.Player1.HasValue)
            RegisterConfig(configSO, PlayerInputManager.Instance.Player1.Value);
        else
            Debug.LogWarning("Can't register config, No Players Detected");
    }

    public void UnregisterConfig(InputConfigSO configSO, InputUser id)
    {
        var inputAssetMapLists = configSO.GetInputAssetMaps();
        foreach (var inputAssetMapList in inputAssetMapLists)
        {
            // skip invalid lists
            if (inputAssetMapList.AssetType == null || !typeof(IInputActionCollection2).IsAssignableFrom(inputAssetMapList.AssetType.Type)) continue;

            foreach (var inputMapStruct in inputAssetMapList.InputMapStructs)
            {
                UnregisterActionMap(id, inputMapStruct);
            }
        }
    }

    public void UnregisterConfig(InputConfigSO configSO)
    {
        if (PlayerInputManager.Instance.Player1.HasValue)
            UnregisterConfig(configSO, PlayerInputManager.Instance.Player1.Value);
        else
            Debug.LogWarning("Can't unregister config, No Players Detected");
    }

    private void CacheActionIfNotAlready(InputUser id, string actionGuid, Type inputAssetCsharp)
    {
        if (!_actionsCaches.ContainsKey(id)) _actionsCaches[id] = new();
        var actionCache = _actionsCaches[id];
        if (!actionCache.ContainsKey(actionGuid))
        {
            // cache value
            if (!_inputBundles.ContainsKey(id)) _inputBundles[id] = new();
            var inputBundle = _inputBundles[id];
            // find inputActionInstance
            IInputActionCollection2 inputActions = inputBundle.GetInputSystemInstance(inputAssetCsharp);
            InputAction inputAction = inputActions.FindAction(actionGuid);
            // InputUser.onChange +=  ;
            // InputUser se;
            // InputUser s = InputUser.CreateUserWithoutPairedDevices();
            // s (inputActions);
            // InputDevice g;
            // inputActions.devices;
            // InputUser.PerformPairingWithDevice()
            actionCache[actionGuid] = inputAction ?? null;
        }
    }

    private void CacheActionIfNotAlready(InputUser id, string actionGuid, IInputActionCollection2 inputActions)
    {
        if (!_actionsCaches.ContainsKey(id)) _actionsCaches[id] = new();
        var actionCache = _actionsCaches[id];
        if (!actionCache.ContainsKey(actionGuid))
        {
            // cache value
            if (!_inputBundles.ContainsKey(id)) _inputBundles[id] = new();
            var inputBundle = _inputBundles[id];
            // find inputActionInstance
            InputAction inputAction = inputActions.FindAction(actionGuid);
            actionCache[actionGuid] = inputAction ?? null;
        }
    }

    private InputAction GetAction(InputUser id, string actionGuid)
    {
        if (_actionsCaches.ContainsKey(id))
        {
            return _actionsCaches[id].ContainsKey(actionGuid) ? _actionsCaches[id][actionGuid] : null;
        }
        return null;
    }

    private void AddEnabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsEnabled.ContainsKey(id)) _actionsEnabled[id] = new();
        if (_actionsEnabled[id].Contains(inputAction)) return;
        _actionsEnabled[id].Add(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnEnabledActionsChanged?.Invoke();
    }

    private void AddDisabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsDisabled.ContainsKey(id)) _actionsDisabled[id] = new();
        if (_actionsDisabled[id].Contains(inputAction)) return;
        _actionsDisabled[id].Add(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnDisabledActionsChanged?.Invoke();
    }

    private void RemoveEnabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsEnabled.ContainsKey(id)) return;
        if (!_actionsEnabled[id].Contains(inputAction)) return;
        _actionsEnabled[id].Remove(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnEnabledActionsChanged?.Invoke();
    }

    private void RemoveDisabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsDisabled.ContainsKey(id)) return;
        if (!_actionsDisabled[id].Contains(inputAction)) return;
        _actionsDisabled[id].Remove(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnDisabledActionsChanged?.Invoke();
    }

    public IReadOnlyList<InputAction> GetEnabledActions(InputUser id)
    {
        return _actionsEnabled[id].AsReadOnly();
    }

    public IReadOnlyList<InputAction> GetDisabledActions(InputUser id)
    {
        return _actionsDisabled[id].AsReadOnly();
    }

    public T GetInputSytemInstanceGeneric<T>(InputUser id) where T : class, IInputActionCollection2, new()
    {
        if (!_inputBundles.ContainsKey(id)) _inputBundles[id] = new();
        var inputBundle = _inputBundles[id];
        T inputSystem = inputBundle.GetInputSystemInstance<T>(out bool createdInstance);

        // if t is a new Instace (so not grabbed from the list of already existing instances)
        // we cache all of the inputActions and set them disabled
        if (createdInstance)
        {
            foreach (var inputAction in inputSystem)
            {
                inputAction.Disable();
                CacheActionIfNotAlready(id, inputAction.id.ToString(), inputSystem);
                AddDisabledAction(id, inputAction);
            }
        }

        return inputSystem;
    }

    public IInputActionCollection2 GetInputSystemInstance(InputUser id, Type type)
    {
        if (!_inputBundles.ContainsKey(id)) _inputBundles[id] = new();
        var inputBundle = _inputBundles[id];
        IInputActionCollection2 inputSystem = inputBundle.GetInputSystemInstance(type, out bool createdInstance);

        // if inputSystem is a new Instace (so not grabbed from the list of already existing instances)
        // we cache all of the inputActions and set them disabled
        if (createdInstance)
        {
            foreach (var inputAction in inputSystem)
            {
                inputAction.Disable();
                CacheActionIfNotAlready(id, inputAction.id.ToString(), inputSystem);
                AddDisabledAction(id, inputAction);
            }
            Debug.Log("Total InputActions: " + inputSystem.Count());
        }

        return inputSystem;
    }

}