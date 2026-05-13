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
    public int CurrentTorchQuantity => _currentTorchQuantity;
    public TorchType SelectedType {  get; private set; }= TorchType.Normal;

    public event Action<GameObject> OnSelectionChange;

    
    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance=this;
        _currentTorchQuantity = TorchMaxQuanitity;

    }
    public void UseTorch() => _currentTorchQuantity--;
    public void ReturnTorch()=>_currentTorchQuantity= Mathf.Min(_currentTorchQuantity+1,TorchMaxQuanitity);

    public bool CanPlace() => _currentTorchQuantity > 0;
    public void SwitchSelection()
    {
        SelectedType= (SelectedType==TorchType.Normal) ? TorchType.Magical:TorchType.Magical;

        GameObject prefabToEquip = (SelectedType == TorchType.Normal) ? TorchPrefab : MagicalTorchPrefab;
        OnSelectionChange?.Invoke(prefabToEquip);
    }
    
}
