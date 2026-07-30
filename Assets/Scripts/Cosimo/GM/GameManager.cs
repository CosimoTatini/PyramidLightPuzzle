using DesignPatterns.Generics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private UiManager _uiManager;
    [SerializeField] private PlacementManager _placementManager;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private GameObject _uiCanvas;
    [SerializeField] private InputConfigSO _blockAllActionsConfig;

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        LevelManager.Instance.OnLoadingStart += HandleLoadingStart;
        LevelManager.Instance.OnLoadingEnd += HandleLoadingEnd;
        LevelManager.Instance.AddSceneWithLoadingScreen("TutorialGabri");
    }
    private void HandleLoadingStart()
    {
        if (_uiCanvas != null)
        {
            _uiCanvas.SetActive(false);
        }
        InputConfigManager.RegisterConfig(_blockAllActionsConfig);
        LevelManager.Instance.OnLoadingStart -= HandleLoadingStart;
    }
    private void HandleLoadingEnd()
    {
        if (_uiCanvas != null && _activateCanvas)
        {
            _uiCanvas.SetActive(true);
        }
        InputConfigManager.UnregisterConfig(_blockAllActionsConfig);
        LevelManager.Instance.OnLoadingEnd -= HandleLoadingEnd;
    }

    public void LoadPyramid()
    {
        LevelManager.Instance.OnLoadingStart += HandleLoadingStart;
        LevelManager.Instance.OnLoadingEnd += HandleLoadingEnd;
        // _uiCanvas.SetActive(false);

        LevelManager.Instance.SwitchGameplayScene("TutorialGabri", "PyramidGabriele");
    }

    private bool _activateCanvas = true;

    public void LoadEndGame()
    {
        LevelManager.Instance.OnLoadingStart += HandleLoadingStart;
        LevelManager.Instance.OnLoadingEnd += HandleLoadingEnd;
        _activateCanvas = false;
        LevelManager.Instance.SwitchGameplayScene("PyramidGabriele", "EndScene");
    }
}
