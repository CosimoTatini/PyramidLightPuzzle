using UnityEngine;
using DesignPatterns;
using DesignPatterns.Generics;
using Unity.VectorGraphics;
using UnityEngine.SceneManagement;

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
        LevelManager.Instance.AddSceneWithLoadingScreen("TutorialCosimo");
    }


}
