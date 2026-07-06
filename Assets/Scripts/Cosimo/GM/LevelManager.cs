using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

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
                            onLoadSceneStart: () => { _loaderCanvas.SetActive(false); },
                            onLoadSceneEnd: () => { }
                        )
                   )
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
        onLoadSceneStart?.Invoke();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, loadMode);

        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            _progressBar.fillAmount = Mathf.Clamp01(operation.progress / 0.9f);
            _progressBar.fillAmount = Mathf.Clamp(_progressBar.fillAmount, 0, _maxProgress);

            _loadingText.text = $"{Mathf.RoundToInt(_progressBar.fillAmount * 100)}%";

            yield return new WaitForEndOfFrame();

            Debug.Log($"{operation.progress}");

            if (operation.progress >= 0.89f)
            {
                Debug.Log("PROGERESS");
                yield return new WaitForSeconds(0.3f);

                int cycles = Mathf.RoundToInt(100 - _maxProgress * 100);
                float speedPerCycle = _progressBarFinalLoadingSpeed / cycles;

                for (int i = 0; i < 100 - _maxProgress * 100; i++)
                {
                    yield return new WaitForSeconds(speedPerCycle);
                    _progressBar.fillAmount += 0.01f;
                    _loadingText.text = $"{Mathf.RoundToInt(_progressBar.fillAmount * 100)}%";
                }
                // add delay here if needed

                yield return new WaitForSeconds(0.3f);

                operation.allowSceneActivation = true;
            }
        }

        onLoadSceneEnd?.Invoke();

    }

    public void QuitGame()
    {
        Debug.Log("QUit");
        Application.Quit();
    }
}
