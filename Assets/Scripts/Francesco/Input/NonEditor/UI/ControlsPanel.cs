using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private ControlRow _rowPrefab;
    [SerializeField] private GameObject _parent;

    private ObjectPooler<ControlRow> _rowsPooler;
    private InputUser _inputUser;

    public void SetUp(InputUser inputUser)
    {
        if (inputUser == null || !inputUser.valid) return;

        // clean an eventual previous inputUser
        if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        {
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateRows;
        }

        _inputUser = inputUser;
        InputConfigManager.EnabledDisabledActionEvents[inputUser].OnEnabledActionsChanged += UpdateRows;
        InputConfigManager.EnabledDisabledActionEvents[inputUser].OnDisabledActionsChanged += UpdateRows;

        UpdateRows();
    }

    void Awake()
    {
        _rowsPooler = new(_rowPrefab);
    }

    void OnEnable()
    {
        if (_inputUser == null || !_inputUser.valid) return;

        if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        {
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnEnabledActionsChanged += UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateRows;
            enabledDisabledAction.OnDisabledActionsChanged += UpdateRows;
        }

        UpdateRows();
    }

    void OnDisable()
    {
        if (_inputUser == null || !_inputUser.valid) return;

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

        for (int i = 0; i < enabledInputActions.Count; i++)
        {
            ControlRow controlRow;
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
            //controlRow.SetUp();
        }

        for (int i = rows.Length - 1; i >= enabledInputActions.Count; i--)
        {
            _rowsPooler.Set(rows[i]);
        }

        if (rows.Length > enabledInputActions.Count)
        {
            // disable extra rows
            for (int i = enabledInputActions.Count; i < rows.Length; i++)
            {
                _rowsPooler.Set(rows[i]);
            }
        }
        else
        {

        }
    }

}
