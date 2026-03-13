
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private ItemSlot [] _itemSlots;
    internal bool AddItemInInventory(string itemName, int quantity, Sprite sprite)
    {
       for(int i = 0; i < _itemSlots.Length; i++)
        {
            if (_itemSlots[i].IsFull == false)
            {
                _itemSlots[i].AddItemInSlot(itemName, quantity, sprite);
                return true;
            }
            

        }
        Debug.Log("Inventario pieno!");
        return false;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            _itemSlots[i].SelectedShader.SetActive(false);
            _itemSlots[i].IsSelected = false;
        }
    }
}

