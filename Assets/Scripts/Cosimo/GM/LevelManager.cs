using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    #region Singleton

    private static LevelManager _instance;

    public static LevelManager Instance
    {
        get
        {
            if (_instance) return _instance;

            _instance = FindFirstObjectByType<LevelManager>(FindObjectsInactive.Include);

            if (_instance) return _instance;

            return Instantiate(Resources.Load<LevelManager>(nameof(LevelManager)), Vector3.zero, Quaternion.identity).GetComponent<LevelManager>();
        }
        set
        {
            _instance = value;
        }
    }
    #endregion
    public bool IsReloadingBattleScene;
    public event Action OnLoadingStart;
    public event Action OnLoadingEnd;
    [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private Image _progressBar;
    [SerializeField, Range(0.01f, 1f)] private float _maxProgress;
    [SerializeField] private float _progressBarFinalLoadingSpeed;
    [SerializeField] private Image _fadePanel;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private float _fadeSpeed;

    [SerializeField] private List<string> _userTips;
    [SerializeField] private TextMeshProUGUI _userTipsText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        IsReloadingBattleScene = false;
        DontDestroyOnLoad(gameObject);
    }

    public void SetImage(Sprite sprite)
    {
        _fadePanel.sprite = sprite;
    }

    public void ChangeScene(string sceneName)
    {
        _loaderCanvas.SetActive(true);

        StartCoroutine
        (
            PreLoadFadeIn
            (
                () => StartCoroutine
                    (
                        LoadSceneAsync
                        (
                            sceneName: sceneName,
                            loadMode: LoadSceneMode.Single,
                            onLoadSceneStart:
                            () =>
                            {
                                _progressBar.gameObject.SetActive(true);
                                _userTipsText.gameObject.SetActive(true);
                                _loadingText.gameObject.SetActive(true);
                                _loadingText.text = "0%";
                                _progressBar.fillAmount = 0;
                                string tip = _userTips[UnityEngine.Random.Range(0, _userTips.Count)];
                                _userTipsText.text = tip;

                            },
                            onLoadSceneEnd:
                            () =>
                            {
                                _progressBar.gameObject.SetActive(false);
                                _userTipsText.gameObject.SetActive(false);
                                _loadingText.gameObject.SetActive(false);
                                StartCoroutine(PostLoadFadeOut(() =>
                                {
                                    _loaderCanvas.SetActive(false);
                                    _fadePanel.gameObject.SetActive(false);
                                }
                            )
                        );
                            }
                        )
                   )
            )
         );
    }

    public void AddScene(string sceneName)
    {
        _loaderCanvas.SetActive(true);
        StartCoroutine
        (
            PreLoadFadeIn
            (
                () => StartCoroutine
                    (
                        LoadSceneAsync
                        (
                            sceneName: sceneName,
                            loadMode: LoadSceneMode.Additive,
                            onLoadSceneStart: () => { },
                            onLoadSceneEnd: () => { _loaderCanvas.SetActive(false); }
                        )
                   )
            )
         );
    }

    public void UnloadScene(string name)
    {
        StartCoroutine(UnloadSceneAsync(name));
    }

    public void AddSceneWithLoadingScreen(string sceneName)
    {
        _loaderCanvas.SetActive(true);

        StartCoroutine
        (
            PreLoadFadeIn
            (
                () =>
                {
                    // Invochiamo l'evento quando il fade in è completato e lo schermo è nero
                    OnLoadingStart?.Invoke();

                    StartCoroutine
                    (
                        LoadSceneAsync
                        (
                            sceneName: sceneName,
                            loadMode: LoadSceneMode.Additive,
                            onLoadSceneStart: () =>
                            {
                                _progressBar.gameObject.SetActive(true);
                                _userTipsText.gameObject.SetActive(true);
                                _loadingText.gameObject.SetActive(true);
                                _loadingText.text = "0%";
                                _progressBar.fillAmount = 0;
                                if (_userTips != null && _userTips.Count > 0)
                                {
                                    string tip = _userTips[UnityEngine.Random.Range(0, _userTips.Count)];
                                    _userTipsText.text = tip;
                                }
                            },
                            onLoadSceneEnd: () =>
                            {
                                _progressBar.gameObject.SetActive(false);
                                _userTipsText.gameObject.SetActive(false);
                                _loadingText.gameObject.SetActive(false);
                                StartCoroutine(PostLoadFadeOut(() =>
                                {
                                    _loaderCanvas.SetActive(false);
                                    _fadePanel.gameObject.SetActive(false);

                                    // Invochiamo l'evento alla fine di tutto, quando il pannello è invisibile
                                    OnLoadingEnd?.Invoke();
                                }));
                            }
                        )
                   );
                }
            )
         );
    }

    private IEnumerator PreLoadFadeIn(Action onPreLoadEnd)
    {
        if (!_fadePanel.gameObject.activeSelf)
            _fadePanel.gameObject.SetActive(true);

        _fadePanel.color = new Color
            (
                r: _fadePanel.color.r,
                g: _fadePanel.color.g,
                b: _fadePanel.color.b,
                a: 0
            );

        while (true)
        {
            yield return new WaitForEndOfFrame();
            _fadePanel.color =
                new Color
                (
                    r: _fadePanel.color.r,
                    g: _fadePanel.color.g,
                    b: _fadePanel.color.b,
                    a: Time.deltaTime * _fadeSpeed + _fadePanel.color.a
                );

            if (_fadePanel.color.a >= 1)
            {
                _fadePanel.color =
                new Color
                (
                    r: _fadePanel.color.r,
                    g: _fadePanel.color.g,
                    b: _fadePanel.color.b,
                    a: 1
                );

                break;
            }
        }

        onPreLoadEnd?.Invoke();
    }

    private IEnumerator PostLoadFadeOut(Action onPostLoadEnd)
    {
        _fadePanel.color = new Color
            (
                r: _fadePanel.color.r,
                g: _fadePanel.color.g,
                b: _fadePanel.color.b,
                a: 1
            );

        while (true)
        {
            yield return new WaitForEndOfFrame();
            _fadePanel.color =
                new Color
                (
                    r: _fadePanel.color.r,
                    g: _fadePanel.color.g,
                    b: _fadePanel.color.b,
                    a: _fadePanel.color.a - Time.deltaTime * _fadeSpeed
                );

            if (_fadePanel.color.a <= 0)
            {
                _fadePanel.color =
                new Color
                (
                    r: _fadePanel.color.r,
                    g: _fadePanel.color.g,
                    b: _fadePanel.color.b,
                    a: 0
                );

                break;
            }
        }

        onPostLoadEnd?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode loadMode, Action onLoadSceneStart, Action onLoadSceneEnd)
    {
        // LOG 1: Verifica quante volte viene avviato effettivamente il caricamento asincrono della scena
        Debug.Log($"[LevelManager] Avvio LoadSceneAsync per la scena: {sceneName} in modalit� {loadMode}");

        onLoadSceneStart?.Invoke();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, loadMode);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            _progressBar.fillAmount = Mathf.Clamp01(operation.progress / 0.9f);
            _progressBar.fillAmount = Mathf.Clamp(_progressBar.fillAmount, 0, _maxProgress);

            string currentTextValue = $"{Mathf.RoundToInt(_progressBar.fillAmount * 100)}%";

            // LOG 2A: Aggiornamento percentuale basato sul progresso reale di Unity
            // Debug.Log($"[LevelManager] Update UI (Progresso Reale) - Fill: {_progressBar.fillAmount} | Testo: {currentTextValue}");

            _loadingText.text = currentTextValue;

            yield return new WaitForEndOfFrame();

            if (operation.progress >= 0.89f)
            {
                yield return new WaitForSeconds(0.3f);

                int cycles = Mathf.RoundToInt(100 - _maxProgress * 100);
                float speedPerCycle = _progressBarFinalLoadingSpeed / cycles;

                for (int i = 0; i < cycles; i++)
                {
                    yield return new WaitForSeconds(speedPerCycle);
                    _progressBar.fillAmount += 0.01f;

                    string fakeTextValue = $"{Mathf.RoundToInt(_progressBar.fillAmount * 100)}%";

                    // LOG 2B: Aggiornamento percentuale nel ciclo for finale fittizio
                    // Debug.Log($"[LevelManager] Update UI (Ciclo Finale) - Step: {i}/{cycles} | Fill: {_progressBar.fillAmount} | Testo: {fakeTextValue}");

                    _loadingText.text = fakeTextValue;
                }

                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }
        }
        Debug.Log($"[LevelManager] Fine LoadSceneAsync. La scena {sceneName} � ora attiva.");
        PlacementManager.NotifySceneLoaded();
        onLoadSceneEnd?.Invoke();
    }

    private IEnumerator UnloadSceneAsync(string name)
    {
        yield return SceneManager.UnloadSceneAsync(name);
    }

    public void SwitchGameplayScene(string currentSceneName, string newSceneName)
    {
        _loaderCanvas.SetActive(true);

        OnLoadingStart?.Invoke();
        StartCoroutine(PreLoadFadeIn(() =>
        {
            StartCoroutine(ExecuteSceneSwitch(currentSceneName, newSceneName));
        }));
    }

    private IEnumerator ExecuteSceneSwitch(string sceneToUnload, string sceneToLoad)
    {
        // 1. CARICHIAMO PRIMA la nuova scena (Additive)
        yield return StartCoroutine(LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive,
            onLoadSceneStart: () =>
            {
                _progressBar.gameObject.SetActive(true);
                _userTipsText.gameObject.SetActive(true);
                _loadingText.gameObject.SetActive(true);
                _loadingText.text = "0%";
                _progressBar.fillAmount = 0;
                if (_userTips != null && _userTips.Count > 0)
                {
                    string tip = _userTips[UnityEngine.Random.Range(0, _userTips.Count)];
                    _userTipsText.text = tip;
                }
            },
            onLoadSceneEnd: () =>
            {
            } // Lasciamo vuoto qui, gestiamo la chiusura alla fine
        ));

        // 2. SCARICHIAMO DOPO la vecchia scena in tutta sicurezza
        if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneToUnload);

            // Controllo di sicurezza per evitare il NullReferenceException
            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }
        }

        // 3. ORA facciamo il fade out e chiudiamo il loader
        _progressBar.gameObject.SetActive(false);
        _userTipsText.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(false);

        StartCoroutine(PostLoadFadeOut(() =>
        {
            _loaderCanvas.SetActive(false);
            _fadePanel.gameObject.SetActive(false);
            OnLoadingEnd?.Invoke();
        }));
    }

    public void QuitGame()
    {
        Debug.Log("QUit");
        Application.Quit();
    }
}
