using DesignPatterns.Generics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{


    [SerializeField] private UiManager _uiManager;
    [SerializeField] private PlacementManager _placementManager;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private GameObject _uiCanvas;

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        LevelManager.Instance.OnLoadingStart += HandleLoadingStart;
        LevelManager.Instance.OnLoadingEnd += HandleLoadingEnd;
        LevelManager.Instance.AddSceneWithLoadingScreen("TutorialFra");
    }
    private void HandleLoadingStart()
    {
        if (_uiCanvas != null)
        {
           _uiCanvas.SetActive(false);
        }
    }
    private void HandleLoadingEnd()
    {
        if (_uiCanvas != null)
        {
          _uiCanvas.SetActive(true);
         }
    }


}
