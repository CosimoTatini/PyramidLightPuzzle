
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private int _quantity;
    [SerializeField] private Sprite _sprite;
    private InventoryManager _inventoryManager;

    private void Start()
    {
        _inventoryManager=GameObject.FindObjectsByType(typeof(InventoryManager),FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<InventoryManager>().First();
        Debug.Log("Find:" + _inventoryManager);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            bool added = _inventoryManager.AddItemInInventory(_itemName, _quantity,_sprite);
            if(added)
            {
                Destroy(gameObject);
            }
            
        }
    }
}
