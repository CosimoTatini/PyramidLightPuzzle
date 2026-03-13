
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private ItemSlot [] _itemSlot;
    internal bool AddItemInInventory(string itemName, int quantity, Sprite sprite)
    {
       for(int i = 0; i < _itemSlot.Length; i++)
        {
            if (_itemSlot[i].IsFull == false)
            {
                _itemSlot[i].AddItemInSlot(itemName, quantity, sprite);
                return true;
            }
            

        }
        Debug.Log("Inventario pieno!");
        return false;
    }
}

