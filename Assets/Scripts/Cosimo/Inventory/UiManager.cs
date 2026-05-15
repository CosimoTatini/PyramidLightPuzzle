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

    public TorchSlotUI TorchSlot;
    public TorchSlotUI MagicTorchSlot;

    public PowderSlotUI RedPowderSlot;
    public PowderSlotUI GreenPowderSlot;
    public PowderSlotUI BluePowderSlot;
    private void OnEnable()
    {
        // Ci iscriviamo all'evento così la UI si aggiorna da sola quando serve
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnPowderChanged += UpdatePowderUI;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnPowderChanged -= UpdatePowderUI;
    }

    private void Start()
    {
        
        UpdatePowderSlot();
    }
    private void Update()
    {
        bool isNormal = InventoryManager.Instance.SelectedType == TorchType.Normal;
        var inv = InventoryManager.Instance;
        var selected = inv.SelectedPowder;

        UpdateTorchSlot(TorchSlot, isNormal, InventoryManager.Instance.CurrentTorchQuantity);
        UpdateTorchSlot(MagicTorchSlot, !isNormal, InventoryManager.Instance.CurrentMagicTorchQuantity);
        UpdatePowderSlot(RedPowderSlot, selected == PowderColor.Red, inv.GetPowderCount(PowderColor.Red));
        UpdatePowderSlot(GreenPowderSlot, selected == PowderColor.Green, inv.GetPowderCount(PowderColor.Green));
        UpdatePowderSlot(BluePowderSlot, selected == PowderColor.Blue, inv.GetPowderCount(PowderColor.Blue));
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
        {
            slot.SelectionImage.SetActive(isActive);
        }
        slot.QuantityText.text = currentCount.ToString();

    }
}
