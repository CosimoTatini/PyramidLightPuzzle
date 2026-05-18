using Assets.Scripts.Cosimo.Inventory;
using System;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Serializable]
    public struct TorchSlotUI
    {
        public GameObject SelectionImage;
        public TextMeshProUGUI QuantityText;
    }

    [Serializable]
    public struct PowderSlotUI
    {
        public GameObject PowderSelectedColor;
        public TextMeshProUGUI QuantityText;
    }

    [Header("Torch Slots")]
    public TorchSlotUI TorchSlot;
    public TorchSlotUI MagicTorchSlot;

    [Header("Powder Slots")]
    public PowderSlotUI RedPowderSlot;
    public PowderSlotUI GreenPowderSlot;
    public PowderSlotUI BluePowderSlot;

    private void Start()
    {
      
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPowderChanged += UpdatePowderUI;
            InventoryManager.Instance.OnTorchChanged += UpdateTorchUI;
        }

       
        UpdateTorchUI();
        UpdatePowderUI();
    }

    private void OnDestroy()
    {
       
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPowderChanged -= UpdatePowderUI;
            InventoryManager.Instance.OnTorchChanged -= UpdateTorchUI;
        }
    }

   
    private void UpdatePowderUI()
    {
        var inv = InventoryManager.Instance;
        var selected = inv.SelectedPowder;

        UpdatePowderSlot(RedPowderSlot, selected == PowderColor.Red, inv.GetPowderCount(PowderColor.Red));
        UpdatePowderSlot(GreenPowderSlot, selected == PowderColor.Green, inv.GetPowderCount(PowderColor.Green));
        UpdatePowderSlot(BluePowderSlot, selected == PowderColor.Blue, inv.GetPowderCount(PowderColor.Blue));
    }


    private void UpdateTorchUI()
    {
        var inv = InventoryManager.Instance;
        bool isNormal = inv.SelectedType == TorchType.Normal;

        UpdateTorchSlot(TorchSlot, isNormal, inv.CurrentTorchQuantity);
        UpdateTorchSlot(MagicTorchSlot, !isNormal, inv.CurrentMagicTorchQuantity);
    }

    private void UpdatePowderSlot(PowderSlotUI slot, bool isActive, int count)
    {
        if (slot.PowderSelectedColor != null)
            slot.PowderSelectedColor.SetActive(isActive);

        slot.QuantityText.text = count.ToString();
    }

    private void UpdateTorchSlot(TorchSlotUI slot, bool isActive, int currentCount)
    {
        if (slot.SelectionImage != null)
            slot.SelectionImage.SetActive(isActive);

        slot.QuantityText.text = currentCount.ToString();
    }
}
