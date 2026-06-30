using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common.Purge;
using DesignPatterns.Generics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public static class InputConfigManager//Singleton<InputConfigManager>
{
    private static Dictionary<InputUser, Dictionary<string, List<InputActionEntry>>> _actionsStacks = new();
    private static Dictionary<InputUser, Dictionary<string, InputAction>> _actionsCaches = new();
    private static Dictionary<InputUser, List<InputAction>> _actionsEnabled = new();
    private static Dictionary<InputUser, List<InputAction>> _actionsDisabled = new();
    private static Dictionary<InputUser, InputBundle> _inputBundles = new();

    private static Dictionary<InputUser, EnabledDisabledAction> _enabledDisabledActionsEvents = new();

    public static Dictionary<InputUser, EnabledDisabledAction> EnabledDisabledActionEvents => _enabledDisabledActionsEvents;

    public class EnabledDisabledAction
    {
        public Action OnEnabledActionsChanged;
        public Action OnDisabledActionsChanged;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _actionsStacks.Clear();
        _actionsCaches.Clear();
        _actionsEnabled.Clear();
        _actionsDisabled.Clear();
        _inputBundles.Clear();
        _enabledDisabledActionsEvents.Clear();

        Application.quitting -= Cleanup;
        Application.quitting += Cleanup;
    }
    private static void Cleanup()
    {
        _actionsStacks.Clear();
        _actionsCaches.Clear();
        _actionsEnabled.Clear();
        _actionsDisabled.Clear();
        _inputBundles.Clear();
        _enabledDisabledActionsEvents.Clear();

        Application.quitting -= Cleanup;
    }

    public static InputActionEntry GetInputActionEntry(InputUser id, string actionGuid)
    {
        if (id == null || !id.valid) return null;
        if (string.IsNullOrEmpty(actionGuid)) return null;

        if (_actionsStacks.TryGetValue(id, out var actionStackDict))
        {
            if (actionStackDict.TryGetValue(actionGuid, out var actionsStack))
            {
                if (actionsStack.Count > 0)
                {
                    return actionsStack[0];
                }
                else
                {
                    return null;
                }
            }
        }

        return null;
    }

    private static void RegisterAction(InputUser id, InputActionEntry actionData/*, Type inputAssetCsharp*/)
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

        //TODO: might as well try to get the InputCction before adding to the stack, if it's null we know this actionData isn't valid
        // since the inputAction would've been instantiated otherwise when First getting the InputActionAsset instace it belongs to

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

        InputActionEntry firstElement = actionStack[0];

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

    private static void UnregisterAction(InputUser id, InputActionEntry actionData)
    {
        if (!_actionsStacks.ContainsKey(id)) return;
        var actionStackDict = _actionsStacks[id];

        if (!actionStackDict.ContainsKey(actionData.Guid)) return;
        var actionStack = actionStackDict[actionData.Guid];

        if (actionStack.Count == 0 || !actionStack.Contains(actionData)) return;

        InputActionEntry firstElement = actionStack[0];

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

    private static void RegisterActionMap(InputUser id, InputMapStruct mapData)
    {
        if (mapData.InputActionEntries.Count == 0) return;
        foreach (var actionData in mapData.InputActionEntries)
        {
            RegisterAction(id, actionData);
        }
    }

    private static void UnregisterActionMap(InputUser id, InputMapStruct mapData)
    {
        if (mapData.InputActionEntries.Count == 0) return;
        foreach (var actionData in mapData.InputActionEntries)
        {
            UnregisterAction(id, actionData);
        }
    }

    public static void RegisterConfig(InputConfigSO configSO, InputUser id)
    {
        // if user is null or there isn't a bundle associated, quit 
        if (id == null || !_inputBundles.ContainsKey(id)) return;

        var inputAssetMapLists = configSO.GetInputAssetMaps();
        foreach (var inputAssetMapList in inputAssetMapLists)
        {
            // skip invalid lists

            // if no type or if type isn't valid, continue
            if (inputAssetMapList.AssetType == null || !typeof(IInputActionCollection2).IsAssignableFrom(inputAssetMapList.AssetType.Type)) continue;

            // if inputBundle doesn't contain a IInputActionCollection2 of type AssetType.Type, then we skip, it means the user didn't need the type yet
            InputBundle inputBundle = _inputBundles[id];
            if (!inputBundle.InputActionCollections.Any(c => c.GetType() == inputAssetMapList.AssetType.Type))
            {
                continue;
            }

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
    public static void RegisterConfig(InputConfigSO configSO)
    {
        if (InputEventsManager.Player1.HasValue)
            RegisterConfig(configSO, InputEventsManager.Player1.Value);
        else
            Debug.LogWarning("Can't register config, No Players Detected");
    }

    public static void UnregisterConfig(InputConfigSO configSO, InputUser id)
    {
        // if user is null or there isn't a bundle associated, quit 
        if (id == null || !_inputBundles.ContainsKey(id)) return;

        var inputAssetMapLists = configSO.GetInputAssetMaps();
        foreach (var inputAssetMapList in inputAssetMapLists)
        {
            // skip invalid lists
            if (inputAssetMapList.AssetType == null || !typeof(IInputActionCollection2).IsAssignableFrom(inputAssetMapList.AssetType.Type)) continue;

            // if inputBundle doesn't contain a IInputActionCollection2 of type AssetType.Type, then we skip, it means the user didn't need the type yet
            InputBundle inputBundle = _inputBundles[id];
            if (!inputBundle.InputActionCollections.Any(c => c.GetType() == inputAssetMapList.AssetType.Type))
            {
                continue;
            }

            foreach (var inputMapStruct in inputAssetMapList.InputMapStructs)
            {
                UnregisterActionMap(id, inputMapStruct);
            }
        }
    }

    public static void UnregisterConfig(InputConfigSO configSO)
    {
        if (InputEventsManager.Player1.HasValue)
            UnregisterConfig(configSO, InputEventsManager.Player1.Value);
        else
            Debug.LogWarning("Can't unregister config, No Players Detected");
    }

    private static void CacheActionIfNotAlready(InputUser id, string actionGuid, Type inputAssetCsharp)
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

    private static void CacheActionIfNotAlready(InputUser id, string actionGuid, IInputActionCollection2 inputActions)
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

    private static InputAction GetAction(InputUser id, string actionGuid)
    {
        if (_actionsCaches.ContainsKey(id))
        {
            return _actionsCaches[id].ContainsKey(actionGuid) ? _actionsCaches[id][actionGuid] : null;
        }
        return null;
    }

    private static void AddEnabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsEnabled.ContainsKey(id)) _actionsEnabled[id] = new();
        if (_actionsEnabled[id].Contains(inputAction)) return;
        _actionsEnabled[id].Add(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnEnabledActionsChanged?.Invoke();
    }

    private static void AddDisabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsDisabled.ContainsKey(id)) _actionsDisabled[id] = new();
        if (_actionsDisabled[id].Contains(inputAction)) return;
        _actionsDisabled[id].Add(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnDisabledActionsChanged?.Invoke();
    }

    private static void RemoveEnabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsEnabled.ContainsKey(id)) return;
        if (!_actionsEnabled[id].Contains(inputAction)) return;
        _actionsEnabled[id].Remove(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnEnabledActionsChanged?.Invoke();
    }

    private static void RemoveDisabledAction(InputUser id, InputAction inputAction)
    {
        if (!_actionsDisabled.ContainsKey(id)) return;
        if (!_actionsDisabled[id].Contains(inputAction)) return;
        _actionsDisabled[id].Remove(inputAction);

        if (!_enabledDisabledActionsEvents.ContainsKey(id)) _enabledDisabledActionsEvents[id] = new();
        _enabledDisabledActionsEvents[id].OnDisabledActionsChanged?.Invoke();
    }

    public static IReadOnlyList<InputAction> GetEnabledActions(InputUser id)
    {
        if (id == null || !_actionsEnabled.ContainsKey(id)) return new List<InputAction>();
        return _actionsEnabled[id].AsReadOnly();
    }

    public static IReadOnlyList<InputAction> GetDisabledActions(InputUser id)
    {
        if (id == null || !_actionsEnabled.ContainsKey(id)) return new List<InputAction>();
        return _actionsDisabled[id].AsReadOnly();
    }

    public static T GetInputSytemInstanceGeneric<T>(InputUser id) where T : class, IInputActionCollection2, new()
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

    public static IInputActionCollection2 GetInputSystemInstance(InputUser id, Type type)
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
            // Debug.Log("Total InputActions: " + inputSystem.Count());
        }

        return inputSystem;
    }

}