using System;
using System.Collections;
using UnityEngine;
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
    }

    private IEnumerator Start()
    {
        // Attendiamo la fine del frame per assicurarci che l'InputSystem del Player sia inizializzato[cite: 3]
        yield return new WaitForEndOfFrame();

        if (_playerController != null)
        {
            _playerController.DisableInput(); // Blocco iniziale dell'input[cite: 3]
        }
    }

    private void OnEnable()
    {
        _startButton?.onClick.AddListener(OnStartButtonClicked);
        _quitButton?.onClick.AddListener(OnQuitButtonCLicked);

    }

    

    private void OnDisable()
    {
        _startButton?.onClick.RemoveListener(OnStartButtonClicked);
        _quitButton?.onClick.RemoveListener(OnQuitButtonCLicked);
    }

    private void OnStartButtonClicked()
    {
        // Disabilitiamo il bottone ed eliminiamo/nascondiamo il menu per sempre
        _startButton.interactable = false;

        if (_menuPanel != null)
        {
            _menuPanel.SetActive(false);
        }

        // Avviamo la transizione della torcia
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

        // Transizione fluida da 0 a 0.4 in 1 secondo
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _fadeDuration;
            _torchLight.intensity = Mathf.Lerp(startIntensity, _targetIntensity, t);
            yield return null; // Attende il frame successivo
        }

        _torchLight.intensity = _targetIntensity;

        // Abilitiamo il giocatore al termine dell'animazione
        EnableGameplay();
    }

    private void EnableGameplay()
    {
        if (_playerController != null)
        {
            _playerController.EnableInput(); // Sblocco dell'input[cite: 3]
        }

        // Il menu ha completato il suo ciclo di vita per questo avvio
        gameObject.SetActive(false);
    }
}