using Assets.Scripts.Cosimo.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the Inventory and how it works.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    [Header("Prefabs")]
    public GameObject TorchPrefab;
    public GameObject MagicalTorchPrefab;
    
    [Header("Settings")]
    public int TorchMaxQuanitity = int.MaxValue;
    public readonly int MagicalTorchQuantity = 1;
    private int _currentTorchQuantity;
    private int _currentMagicalTorchQuantity;
    public int CurrentTorchQuantity => _currentTorchQuantity;
    public int CurrentMagicTorchQuantity => _currentMagicalTorchQuantity;
    public TorchType SelectedType {  get; private set; }= TorchType.Normal;

    public event Action<GameObject> OnSelectionChange;
    public event Action OnTorchChanged;//tengo conto dei consumi e dei ritorni delle torce con un evento

    private Dictionary<PowderColor, int> _powders= new Dictionary<PowderColor, int>()
    {
        {PowderColor.Red,4 },
        {PowderColor.Green,4},
        {PowderColor.Blue,4},
    };
    public PowderColor SelectedPowder {  get; private set; } = PowderColor.Red;
    public event Action OnPowderChanged;

    #region SINGLETON_INSTANCE
    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance=this;
        _currentTorchQuantity = 4;
        _currentMagicalTorchQuantity= 1;

    }
    #endregion

    #region TORCH_METHODS
    /// <summary>
    /// Decrease and use torches.
    /// </summary>
    public void UseTorch()
    {
        if(SelectedType== TorchType.Normal)
        {
            _currentTorchQuantity--;
        }
        else
        {
            _currentMagicalTorchQuantity--;
        }
        OnTorchChanged?.Invoke();
    }
    /// <summary>
    /// Can return the torches to the inventory by grabbing them and increase the quantity
    /// </summary>
    /// <param name="type"></param>
    public void ReturnTorch(TorchType type)
    {
        if(type==TorchType.Normal)
        {
            _currentTorchQuantity= Mathf.Min(_currentTorchQuantity+1, TorchMaxQuanitity);
        }

        if(type==TorchType.Magical)
        {
            _currentMagicalTorchQuantity=Mathf.Min(_currentMagicalTorchQuantity+1, MagicalTorchQuantity);
        }
        OnTorchChanged?.Invoke();
    }

    /// <summary>
    /// Verify that player can place only if has at least 1 torch of any type.
    /// </summary>
    /// <returns></returns>
    public bool CanPlace()
    {
        if(SelectedType==TorchType.Normal)
        {
            return _currentTorchQuantity > 0;
        }

        else
        {
          return _currentMagicalTorchQuantity > 0;
        }
    }
    /// <summary>
    /// This event change the type selected in UI and his spawn
    /// </summary>
    public void SwitchSelection()
    {
        SelectedType= (SelectedType==TorchType.Normal) ? TorchType.Magical:TorchType.Normal;

        GameObject prefabToEquip = (SelectedType == TorchType.Normal) ? TorchPrefab : MagicalTorchPrefab;
        OnSelectionChange?.Invoke(prefabToEquip);
        OnTorchChanged?.Invoke();
    }

  
    #endregion

    #region POWDER_METHODS

    /// <summary>
    /// This method increase the powders quantity of a certain type by grabbing them
    /// </summary>
    /// <param name="color"></param>
    /// <param name="amount"></param>
    public void AddPowder(PowderColor color,int amount)
    {
        if(_powders.ContainsKey(color))
        {
            _powders[color] += amount;
            OnPowderChanged?.Invoke();
        }
    }
    /// <summary>
    /// Shows the Powder quantity of a certain color
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public int GetPowderCount(PowderColor color) => _powders[color];
    
    /// <summary>
    /// Show the Selection of a powder of a certain color.
    /// It's linked to the R,G,B keys.
    /// </summary>
    /// <param name="color"></param>
    public void SelectPowder(PowderColor color)
    {
        SelectedPowder = color;
        OnPowderChanged?.Invoke();
    }

    /// <summary>
    /// This method goes to previous/next selection of powder.
    /// It is linked to K,O keys.
    /// </summary>
    /// <param name="direction"></param>
    
    public void CyclePowder(int direction)
    {
        int totalColors= Enum.GetValues(typeof(PowderColor)).Length;
        int nextIndex = ((int) SelectedPowder+direction+totalColors) % totalColors;
        SelectPowder((PowderColor)nextIndex);
    }

    /// <summary>
    /// Player can throw if at least has 1 powder of any type.
    /// </summary>
    /// <returns></returns>
    public bool CanThrowPowder() => _powders[SelectedPowder] > 0;

    /// <summary>
    /// If at least as 1, use it
    /// </summary>
    public void UsePowder()
    {
        if(CanThrowPowder())
        {
            //TODO:When magical torch is ready fully, change also the RGB values +1 based on the type of used powder
            _powders[SelectedPowder]--;
            OnPowderChanged?.Invoke();
        }
    }

    

    #endregion

}
