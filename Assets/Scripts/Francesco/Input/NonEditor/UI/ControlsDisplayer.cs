using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class ControlsDisplayer : MonoBehaviour
{
    [SerializeField] private ControlRow _rowPrefab;
    [SerializeField] private GameObject _parent;
    [SerializeField] private SpriteAssetsInputList _spriteAssetsInputList;
    [SerializeField] private InputConfigSO _displayedActions;

    private InputUser _inputUser;
    private InputControlScheme? _currentScheme;
    private ObjectPooler<ControlRow> _rowsPooler;
    private HashSet<string> _displayedActionGuids;
    private Dictionary<string, InputActionEntry> _actionsEntries;

    // from the InputConfigSO by using TypeVars grab all of instances that are required from that action
    // to display them we just going to
    private List<KeyValuePair<Type, IInputActionCollection2>> _inputActionAssets;

    public void SchemeChanged(InputUser inputUser, InputDevice device = null)
    {
        if (inputUser == null || !inputUser.valid) return;
        if (inputUser != _inputUser) return;
        _currentScheme = inputUser.controlScheme;
        UpdateRows();
    }

    void Awake()
    {
        GameObject poolParent = new();
        poolParent.transform.SetParent(transform);
        poolParent.transform.localPosition = Vector3.zero;
        _rowsPooler = new(_rowPrefab, parent: poolParent.transform, poolName: "ControlsDisplayer");

        if (InputUser.all.Count > 0)
        {
            _inputUser = InputUserEventsManager.Player1.Value;
        }

        if (_inputUser != null)
        {
            _displayedActionGuids = new();
            _actionsEntries = new();

            if (_displayedActions != null)
            {
                var inputEntries = _displayedActions.GetInputAssetMaps()
                    .SelectMany(k => k.InputMapStructs)
                    .SelectMany(k => k.InputActionEntries).ToArray();

                for (int i = 0; i < inputEntries.Length; i++)
                {
                    var entry = inputEntries[i];
                    _actionsEntries[entry.Guid] = entry;
                }
                _displayedActionGuids = _actionsEntries.Keys.ToHashSet();
            }

            _inputActionAssets = new();

            var inputAssetMaps = _displayedActions.GetInputAssetMaps();
            for (int i = 0; i < inputAssetMaps.Count; i++)
            {
                InputAssetMapList inputAssetMap = inputAssetMaps[i];
                if (inputAssetMap.AssetType == null
                || inputAssetMap.AssetType.Type == null
                || !typeof(IInputActionCollection2).IsAssignableFrom(inputAssetMap.AssetType.Type)) continue;

                _inputActionAssets.Add(new(inputAssetMap.AssetType.Type, InputConfigManager.GetInputSystemInstance(_inputUser, inputAssetMap.AssetType.Type)));
            }

            SchemeChanged(_inputUser);
        }
    }

    void OnEnable()
    {
        // if (_inputUser == null || !_inputUser.valid) return;

        InputUserEventsManager.OnControlSchemeChanged -= SchemeChanged;
        InputUserEventsManager.OnControlSchemeChanged += SchemeChanged;

        SchemeChanged(_inputUser);
    }

    void OnDisable()
    {
        // if (_inputUser == null || !_inputUser.valid) return;
        InputUserEventsManager.OnControlSchemeChanged -= SchemeChanged;

        // if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        // {
        //     enabledDisabledAction.OnEnabledActionsChanged -= UpdateRows;
        //     enabledDisabledAction.OnDisabledActionsChanged -= UpdateRows;
        // }
    }

    private void UpdateRows()
    {
        var rows = _parent.GetComponentsInChildren<ControlRow>();

        if (!_currentScheme.HasValue || _inputUser == null || !_inputUser.valid)
        {
            for (int i = rows.Length - 1; i >= 0; i--)
            {
                _rowsPooler.Set(rows[i]);
            }
            return;
        }

        HashSet<int> skippedActions = new();

        int rowIndex = 0;
        for (int i = 0; i < _inputActionAssets.Count; i++)
        {
            var inputAssetInstance = _inputActionAssets[i].Value;
            foreach (var inputAction in inputAssetInstance)
            {
                string actionGuid = inputAction.id.ToString();
                if (!_displayedActionGuids.Contains(actionGuid))
                {
                    skippedActions.Add(rowIndex++);
                    continue;
                }

                ControlRow controlRow;
                InputActionEntry overrideEntry = _actionsEntries.ContainsKey(actionGuid) ? _actionsEntries[inputAction.id.ToString()] : null;
                string bindingPrompt = InputActionEntry.GetActionTextWithSprites(inputAction, _inputUser, _currentScheme, _spriteAssetsInputList, out _, overrideEntry);

                // we have already spawned rows, use them
                if (rowIndex < rows.Length)
                {
                    controlRow = rows[rowIndex];
                }
                // spawn a new row
                else
                {
                    controlRow = _rowsPooler.Get(_parent.transform);
                }

                // I can get the name override and the "Press @BUTTON to interact" from inputActionStruct, i just need to grab the first one in _actionsStacks
                controlRow.Initialize(bindingPrompt);
                rowIndex++;
            }
        }

        // disable the skipped rows and the extra ones if any
        foreach (var index in skippedActions)
        {
            if (index < rows.Length)
                _rowsPooler.Set(rows[index]);
        }
        for (int i = rows.Length - 1; i >= rowIndex; i--)
        {
            _rowsPooler.Set(rows[i]);
        }
    }
}
