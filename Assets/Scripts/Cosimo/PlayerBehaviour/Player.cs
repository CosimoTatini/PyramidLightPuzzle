
using UnityEngine;
public class Player : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    private void Awake()
    {
        _inventoryManager = FindFirstObjectByType <InventoryManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item>(out Item item ))
        {
            bool added = _inventoryManager.AddItemInInventory(item.ItemName,item.Quantity,item.Sprite,item.ItemDescription);
            Debug.Log(added);
            if (added)
            {
                Destroy(collision.gameObject);
            }

        }
    }
}
