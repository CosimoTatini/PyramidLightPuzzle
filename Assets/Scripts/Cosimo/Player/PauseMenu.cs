using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _pauseMenuParent;
    [SerializeField] private SingleButtonControlDisplayer _pauseButtonControlDisplayer;
    
    [Header("Resume")]
    [SerializeField] private Button _resumeButton;
    
    [Header("Quit")]
    [SerializeField] private Button _quitButton;

    [Header("Options")]
    [SerializeField] private Button _optionsButton;
    [SerializeField] private GameObject _optionsParent;
    [SerializeField] private Button _optionsCloseButton;
    [SerializeField] private SingleButtonControlDisplayer _optionsCloseButtonControlDisplayer;
    
    [Header("Keybindings")]
    [SerializeField] private Button _keybindingsButton;
    [SerializeField] private GameObject _keybindingsParent;
    [SerializeField] private Button _keybindingsCloseButton;
    [SerializeField] private SingleButtonControlDisplayer _keybindingsButtonControlDisplayer;
    
    [Header("Game UI")]
    [SerializeField] private GameObject _inGameUI;
    [SerializeField] private InputConfigSO _zoomDisableConfig;

    void OnEnable()
    {
        _pauseButton.onClick.RemoveListener(PauseButtonClicked);
        _pauseButton.onClick.AddListener(PauseButtonClicked);

        _resumeButton.onClick.RemoveListener(PauseButtonClicked);
        _resumeButton.onClick.AddListener(PauseButtonClicked);
        
        _quitButton.onClick.RemoveListener(Quit);
        _quitButton.onClick.AddListener(Quit);
        
        _optionsButton.onClick.RemoveListener(OptionsButtonClicked);
        _optionsButton.onClick.AddListener(OptionsButtonClicked);
        _optionsCloseButton.onClick.RemoveListener(OptionsCloseButtonClicked);
        _optionsCloseButton.onClick.AddListener(OptionsCloseButtonClicked);
        
        _keybindingsButton.onClick.RemoveListener(KeybindingsButtonClicked);
        _keybindingsButton.onClick.AddListener(KeybindingsButtonClicked);
        _keybindingsCloseButton.onClick.RemoveListener(KeybindingsCloseButtonClicked);
        _keybindingsCloseButton.onClick.AddListener(KeybindingsCloseButtonClicked);
    }

    private void KeybindingsCloseButtonClicked()
    {
        _pauseButtonControlDisplayer.Activate();
    }

    private void OptionsCloseButtonClicked()
    {
        _pauseButtonControlDisplayer.Activate();
    }

    void OnDisable()
    {
        _pauseButton.onClick.RemoveListener(PauseButtonClicked);

        _resumeButton.onClick.RemoveListener(PauseButtonClicked);
        
        _quitButton.onClick.RemoveListener(Quit);
        
        _optionsButton.onClick.RemoveListener(OptionsButtonClicked);
        
        _optionsCloseButton.onClick.RemoveListener(OptionsCloseButtonClicked);
        
        _keybindingsButton.onClick.RemoveListener(KeybindingsButtonClicked);
        
        _keybindingsCloseButton.onClick.RemoveListener(KeybindingsCloseButtonClicked);
    }


    private void KeybindingsButtonClicked()
    {
        bool keybindingsOpen = _keybindingsParent.activeSelf;

        _keybindingsParent.SetActive(!keybindingsOpen);
        if (keybindingsOpen)
        {
            FocusPanel(_pauseMenuParent, _resumeButton.gameObject);
            _pauseButtonControlDisplayer.Activate();
        }
        else
        {
            FocusPanel(_keybindingsParent);
            _pauseButtonControlDisplayer.Deactivate();
        }
    }

    private void OptionsButtonClicked()
    {
        bool optionsOpen = _optionsParent.activeSelf;
        _optionsParent.SetActive(!optionsOpen);
        if (optionsOpen)
        {
            FocusPanel(_pauseMenuParent, _resumeButton.gameObject);
            _pauseButtonControlDisplayer.Activate();
        }
        else
        {
            FocusPanel(_optionsParent);
            _pauseButtonControlDisplayer.Deactivate();
        }
    }

    private void Quit()
    {
        Application.Quit();
    }


    private void PauseButtonClicked()
    {
        bool menuOpen = _pauseMenuParent.activeSelf;
        _pauseMenuParent.SetActive(!menuOpen);
        _inGameUI.SetActive(menuOpen);

        if (menuOpen)
        {
            Time.timeScale = 1;
            EventSystem.current.SetSelectedGameObject(null);
            InputConfigManager.UnregisterConfig(_zoomDisableConfig);
        }
        else
        {
            Time.timeScale = 0;
            FocusPanel(_pauseMenuParent, null);
            InputConfigManager.RegisterConfig(_zoomDisableConfig);
        }
    }

    public void FocusPanel(GameObject panel)
    {
        FocusPanel(panel, null);
    }

    public void FocusPanel(GameObject panel, GameObject defaultSelectable = null)
    {
        if (panel == null || !panel.activeInHierarchy)
        {
            return;
        }

        if (defaultSelectable == null || !defaultSelectable.activeInHierarchy)
        {
            defaultSelectable = FindFirstSelectable(panel);
        }

        if (defaultSelectable == null || !defaultSelectable.activeInHierarchy || !defaultSelectable.TryGetComponent(out Selectable _))
        {
            return;
        }

        Debug.Log("Focus " + defaultSelectable);

        FocusSelectable(defaultSelectable);
    }

    private void FocusSelectable(GameObject selectable)
    {
        if (selectable == null)
        {
            return;
        }

        if (!selectable.activeInHierarchy)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectable);
    }

    private GameObject FindFirstSelectable(GameObject panel)
    {
        if (panel == null) return null;

        Selectable[] selectables = panel.GetComponentsInChildren<Selectable>(true);

        for (int i = 0; i < selectables.Length; i++)
        {
            Selectable selectable = selectables[i];
            if (selectable.gameObject.activeInHierarchy && selectable.interactable)
            {
                return selectable.gameObject;
            }
        }

        return null;
    }
}
