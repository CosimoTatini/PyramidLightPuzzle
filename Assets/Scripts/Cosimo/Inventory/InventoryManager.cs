using Assets.Scripts.Cosimo.Inventory;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  public static InventoryManager Instance;

    [Header("Settings")]
    public int TorchMaxQuanitity = 5;
    public readonly int MagicalTorchQuantity = 1;

    private Dictionary<PowderColor,int> _powders= new Dictionary<PowderColor,int>();

    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance=this;

        foreach(PowderColor color in Enum.GetValues(typeof(PowderColor)))
        {
            _powders[color] = 0;
        }
    }

    public void AddPowder(PowderColor color,int amount) => _powders[color] += amount;




    public int GetPowder(PowderColor color) => _powders[color]; 


    public bool ConsumePowder(PowderColor color)
    {
        if (_powders[color])
    }
    
}
