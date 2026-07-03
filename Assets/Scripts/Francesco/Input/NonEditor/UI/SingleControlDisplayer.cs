using System;
using System.Linq;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class SingleControlDisplayer : MonoBehaviour
{
    [Tooltip("Only first action is taken into account")]
    [SerializeField] private InputConfigSO _config;
    [SerializeField] private TextMeshProUGUI _textControl;
    [SerializeField, Range(0f, 1f)] private float _disabledOpacity;
    [SerializeField] private SpriteAssetsInputList _spriteAssetsInputList;

    [SerializeField, Min(1)] private int _playerNumber;

    public int PlayerNumber
    {
        get
        {
            return _playerNumber;
        }
        set
        {
            if (value < 1) value = 1;
            _playerNumber = value;
        }
    }

    private InputUser _inputUser;
    private InputControlScheme? _currentScheme;
    private InputActionEntry _firstActionEntry;

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
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateText;
            enabledDisabledAction.OnEnabledActionsChanged += UpdateText;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateText;
            enabledDisabledAction.OnDisabledActionsChanged += UpdateText;
        }

        // InputConfigManager.EnabledDisabledActionEvents[inputUser].OnEnabledActionsChanged += UpdateRows;
        // InputConfigManager.EnabledDisabledActionEvents[inputUser].OnDisabledActionsChanged += UpdateRows;

        UpdateText();
    }

    public void SchemeChanged(InputUser inputUser, InputDevice device = null)
    {
        if (inputUser == null || !inputUser.valid) return;
        if (inputUser != _inputUser) return;
        _currentScheme = inputUser.controlScheme;
        UpdateText();
    }

    void Awake()
    {
        if (_config != null)
        {
            var allConfigActionsGuids = _config.GetInputAssetMaps()
                .SelectMany(k => k.InputMapStructs)
                .SelectMany(k => k.InputActionEntries);

            if (allConfigActionsGuids.Count() > 0)
            {
                _firstActionEntry = allConfigActionsGuids.ElementAt(0);
            }
            else
            {
                _firstActionEntry = null;
            }
        }
        else
        {
            _firstActionEntry = null;
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
        InputEventsManager.OnUserAdded -= PlayerSetUp;
        InputEventsManager.OnUserAdded += PlayerSetUp;
        InputEventsManager.OnControlSchemeChanged -= SchemeChanged;
        InputEventsManager.OnControlSchemeChanged += SchemeChanged;
    }

    void OnDisable()
    {
        InputEventsManager.OnUserAdded -= PlayerSetUp;
        InputEventsManager.OnControlSchemeChanged -= SchemeChanged;

        if (InputConfigManager.EnabledDisabledActionEvents.TryGetValue(_inputUser, out var enabledDisabledAction))
        {
            enabledDisabledAction.OnEnabledActionsChanged -= UpdateText;
            enabledDisabledAction.OnDisabledActionsChanged -= UpdateText;
        }
    }


    private void UpdateText()
    {
        Color textControlColor = _textControl.color;

        if (_inputUser == null || !_inputUser.valid)
        {
            _textControl.text = $"User problem";
            textControlColor.a = 1;
            _textControl.color = textControlColor;
            return;
        }
        if (_firstActionEntry == null)
        {
            _textControl.text = $"Config problem";
            textControlColor.a = 1;
            _textControl.color = textControlColor;
            return;
        }

        var enabledInputActions = InputConfigManager.GetEnabledActions(_inputUser);
        var disabledInputActions = InputConfigManager.GetDisabledActions(_inputUser);
        int indexInEnabledActions = Array.IndexOf(enabledInputActions.Select(a => a.id.ToString()).ToArray(), _firstActionEntry.Guid);
        if (indexInEnabledActions != -1)
        {
            string bindingPrompt = InputActionEntry.GetActionTextWithSprites(enabledInputActions[indexInEnabledActions], _inputUser, _currentScheme, _spriteAssetsInputList, out _, _firstActionEntry);
            _textControl.text = bindingPrompt;

            textControlColor.a = 1;
            _textControl.color = textControlColor;
            return;
        }

        int indexInDisabledActions = Array.IndexOf(disabledInputActions.Select(a => a.id.ToString()).ToArray(), _firstActionEntry.Guid);
        if (indexInDisabledActions != -1)
        {
            string bindingPrompt = InputActionEntry.GetActionTextWithSprites(disabledInputActions[indexInDisabledActions], _inputUser, _currentScheme, _spriteAssetsInputList, out _, _firstActionEntry);
            _textControl.text = bindingPrompt;

            textControlColor.a = _disabledOpacity;
            _textControl.color = textControlColor;
            return;
        }

        _textControl.text = $"Can't find {_firstActionEntry?.Name}";
        textControlColor.a = 1;
        _textControl.color = textControlColor;
    }
}
