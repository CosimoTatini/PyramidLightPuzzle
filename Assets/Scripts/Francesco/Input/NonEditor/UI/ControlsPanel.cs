using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private ControlRow _rowPrefab;
    [SerializeField] private GameObject _parent;
    [Tooltip("Links this controls panel to a specific player number")]
    [SerializeField, Min(1)] private int _playerNumber;
    [SerializeField] private SpriteAssetsInputList _spriteAssetsInputList;
    [SerializeField] private InputConfigSO _excludedActions;

    public int PlayerNumber
    {
        get
        {
            return _playerNumber;
        }
        set
        {
            if(value < 1) value = 1;
            _playerNumber = value;
        }
    }

    private ObjectPooler<ControlRow> _rowsPooler;
    private InputUser _inputUser;
    private InputControlScheme? _currentScheme;

    private HashSet<string> _excludedActionGuids;

    public void PlayerSetUp(InputUser inputUser, InputDevice device = null)
    {
        if (inputUser == null || !inputUser.valid) return;

        if (_playerNumber == InputUser.all.Count)
        {
            _inputUser = inputUser;
        }
        else
        {
            return;
        }

        if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        {
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnEnabledActionsChanged += UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged += UpdateRows;
        }

        // InputConfigManager.EnabledDisabledActionEvents[inputUser].OnEnabledActionsChanged += UpdateRows;
        // InputConfigManager.EnabledDisabledActionEvents[inputUser].OnDisabledActionsChanged += UpdateRows;

        UpdateRows();
    }

    public void SchemeChanged(InputUser inputUser, InputDevice device = null)
    {
        if (inputUser == null || !inputUser.valid) return;
        if (inputUser != _inputUser) return;
        _currentScheme = inputUser.controlScheme;
        UpdateRows();
    }

    void Awake()
    {
        _rowsPooler = new(_rowPrefab);
        if (_excludedActions != null)
        {
            _excludedActionGuids = _excludedActions.GetInputAssetMaps()
                .SelectMany(k => k.InputMapStructs)
                .SelectMany(k => k.InputActionEntries)
                .Select(k => k.Guid)
                .ToHashSet();
        }
        else
        {
            _excludedActionGuids = new();
        }
        if (InputUser.all.Count > 0)
        {
            foreach (var inputUser in InputUser.all)
            {
                PlayerSetUp(inputUser);
            }
        }

        if (_inputUser != null)
        {
            SchemeChanged(_inputUser);
        }
    }

    void OnEnable()
    {
        // if (_inputUser == null || !_inputUser.valid) return;

        InputEventsManager.OnUserAdded -= PlayerSetUp;
        InputEventsManager.OnUserAdded += PlayerSetUp;
        InputEventsManager.OnControlSchemeChanged -= SchemeChanged;
        InputEventsManager.OnControlSchemeChanged += SchemeChanged;
    }

    void OnDisable()
    {
        // if (_inputUser == null || !_inputUser.valid) return;
        InputEventsManager.OnUserAdded -= PlayerSetUp;
        InputEventsManager.OnControlSchemeChanged -= SchemeChanged;

        if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        {
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateRows;
        }
    }

    private void UpdateRows()
    {
        var rows = _parent.GetComponentsInChildren<ControlRow>();

        var enabledInputActions = InputConfigManager.GetEnabledActions(_inputUser);

        HashSet<int> skippedActions = new();
        if (!_currentScheme.HasValue)
        {
            goto PoolUnusedRows;
        }

        for (int i = 0; i < enabledInputActions.Count; i++)
        {
            InputAction inputAction = enabledInputActions[i];

            // skip action if excluded
            if (_excludedActionGuids.Contains(inputAction.id.ToString()))
            {
                skippedActions.Add(i);
                continue;
            }

            ControlRow controlRow;
            string bindingPrompt = InputActionEntry.GetActionTextWithSprites(inputAction, _inputUser, _currentScheme, _spriteAssetsInputList, out _);

            // we have already spawned rows, use them
            if (i < rows.Length)
            {
                controlRow = rows[i];
            }
            // spawn a new row
            else
            {
                controlRow = _rowsPooler.Get(_parent.transform);
            }

            // I can get the name override and the "Press @BUTTON to interact" from inputActionStruct, i just need to grab the first one in _actionsStacks
            controlRow.Initialize(bindingPrompt);
        }


    PoolUnusedRows:

        // disable the skipped rows and the extra ones if any
        foreach (var index in skippedActions)
        {
            if (index < rows.Length)
                _rowsPooler.Set(rows[index]);
        }
        for (int i = rows.Length - 1; i >= enabledInputActions.Count; i--)
        {
            _rowsPooler.Set(rows[i]);
        }
    }
}
