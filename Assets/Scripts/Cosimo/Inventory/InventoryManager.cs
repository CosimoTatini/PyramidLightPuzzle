using Assets.Scripts.Cosimo.Inventory;
using Codice.Client.Common.GameUI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    [Header("Prefabs")]
    public GameObject TorchPrefab;
    public GameObject MagicalTorchPrefab;
    
    [Header("Settings")]
    public readonly int TorchMaxQuanitity = 4;
    public readonly int MagicalTorchQuantity = 1;
    private int _currentTorchQuantity;
    private int _currentMagicalTorchQuantity;
    public int CurrentTorchQuantity => _currentTorchQuantity;
    public int CurrentMagicTorchQuantity => _currentMagicalTorchQuantity;
    public TorchType SelectedType {  get; private set; }= TorchType.Normal;

    public event Action<GameObject> OnSelectionChange;

    private Dictionary<PowderColor, int> _powders= new Dictionary<PowderColor, int>()
    {
        {PowderColor.Red,5 },
        {PowderColor.Green,5},
        {PowderColor.Blue,5},
    };
    public PowderColor SelectedPowder {  get; private set; } = PowderColor.Red;
    public event Action OnPowderChanged;
    
    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance=this;
        _currentTorchQuantity = TorchMaxQuanitity;
        _currentMagicalTorchQuantity= MagicalTorchQuantity;

    }

    #region TORCH_METHODS
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
    }
    public void ReturnTorch(TorchType type)
    {
        if(type==TorchType.Normal)
        {
            _currentTorchQuantity= Mathf.Min(_currentTorchQuantity+1, TorchMaxQuanitity);
        }
        else
        {
            _currentMagicalTorchQuantity = Mathf.Min(_currentMagicalTorchQuantity + 1, MagicalTorchQuantity);
        }
    }

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
    public void SwitchSelection()
    {
        SelectedType= (SelectedType==TorchType.Normal) ? TorchType.Magical:TorchType.Normal;

        GameObject prefabToEquip = (SelectedType == TorchType.Normal) ? TorchPrefab : MagicalTorchPrefab;
        OnSelectionChange?.Invoke(prefabToEquip);
    }
    #endregion

    #region POWDER_METHODS
    public int GetPowderCount(PowderColor color) => _powders[color];
    
    public void SelectPowder(PowderColor color)
    {
        SelectedPowder = color;
        OnPowderChanged?.Invoke();
    }


    public void CyclePowder(int direction)
    {
        int totalColors= Enum.GetValues(typeof(PowderColor)).Length;
        int nextIndex = ((int) SelectedPowder+direction+totalColors) % totalColors;
        SelectPowder((PowderColor)nextIndex);
    }

    public bool CanThrowPowder() => _powders[SelectedPowder] > 0;


    public void UsePowder()
    {
        if(CanThrowPowder())
        {
            _powders[SelectedPowder]--;
            OnPowderChanged?.Invoke();
        }
    }

    #endregion

}
