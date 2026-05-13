using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Serializable]
    public struct SlotUI
    {
        public GameObject SelectionImage;
        public Image ObjectImage;
        public TextMeshProUGUI quantityText;
    }

    public SlotUI TorchSlot;
    public SlotUI MagicTorchSlot;

    private void Update()
    {
        bool isNormal = InventoryManager.Instance.SelectedType == TorchType.Normal;

        UpdateSlot(TorchSlot, isNormal, InventoryManager.Instance.TorchMaxQuanitity);
        UpdateSlot(MagicTorchSlot, !isNormal, InventoryManager.Instance.MagicalTorchQuantity);
    }

    private void UpdateSlot(SlotUI slot, bool isActive, int currentCount)
    {
       if(slot.SelectionImage != null)
       {
            slot.SelectionImage.SetActive(isActive);
       }
       slot.quantityText.text= currentCount.ToString();

       if (slot.ObjectImage != null)
        {
            slot.ObjectImage.color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
        }
    }
}
