using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Gestisce il menu overlay di partenza, la dissolvenza della torcia e lo sblocco del Player.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;

    [Header("Torch Light Settings")]
    [SerializeField] private Light2D _torchLight;
    [SerializeField] private float _targetIntensity = 0.4f;
    [SerializeField] private float _fadeDuration = 1.0f;

    [Header("Player Reference")]
    [SerializeField] private PlayerController _playerController;

    private void Awake()
    {
        // Sicurezza iniziale: torcia spenta
        if (_torchLight != null)
        {
            _torchLight.intensity = 0f;
        }

        _startButton.navigation = new()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = _quitButton,
            selectOnUp = null,
            selectOnLeft = null,
            selectOnRight = null,
        };
        var startNavigation = _startButton.navigation;
        startNavigation.wrapAround = true;
        _startButton.navigation = startNavigation;

        _quitButton.navigation = new()
        {
            mode = Navigation.Mode.Explicit,
            selectOnDown = null,
            selectOnUp = _startButton,
            selectOnLeft = null,
            selectOnRight = null,
        };
        var quitNavigation = _quitButton.navigation;
        quitNavigation.wrapAround = true;
        _quitButton.navigation = quitNavigation;

    }

    private IEnumerator Start()
    {
        // Attendiamo la fine del frame per assicurarci che l'InputSystem del Player sia inizializzato
        yield return new WaitForEndOfFrame();

        if (_playerController != null)
        {
            _playerController.DisableInput(); // Blocco iniziale dell'input
        }
    }

    private void OnEnable()
    {
        _startButton?.onClick.AddListener(OnStartButtonClicked);
        _quitButton?.onClick.AddListener(OnQuitButtonCLicked);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
    }



    private void OnDisable()
    {
        _startButton?.onClick.RemoveListener(OnStartButtonClicked);
        _quitButton?.onClick.RemoveListener(OnQuitButtonCLicked);
    }

    private void OnStartButtonClicked()
    {

        _startButton.interactable = false;

        if (_menuPanel != null)
        {
            _menuPanel.SetActive(false);
        }


        StartCoroutine(TurnOnTorchRoutine());
    }
    private void OnQuitButtonCLicked()
    {
        Application.Quit();
    }

    private IEnumerator TurnOnTorchRoutine()
    {
        if (_torchLight == null)
        {
            Debug.LogWarning("[MainMenuManager] Riferimento alla torcia mancante!");
            EnableGameplay();
            yield break;
        }

        float elapsedTime = 0f;
        float startIntensity = _torchLight.intensity;


        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _fadeDuration;
            _torchLight.intensity = Mathf.Lerp(startIntensity, _targetIntensity, t);
            yield return null;
        }

        _torchLight.intensity = _targetIntensity;


        EnableGameplay();
    }

    private void EnableGameplay()
    {
        if (_playerController != null)
        {
            _playerController.EnableInput();
        }


        gameObject.SetActive(false);
    }
}