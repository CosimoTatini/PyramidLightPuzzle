
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private int _quantity;
    [SerializeField] private Sprite _sprite;
    //// Only for debug purpose
    //[SerializeField] private Color _color;
    private InventoryManager _inventoryManager;

    [TextArea]
    [SerializeField] private string _itemDescription;

    private void Start()
    {
        _inventoryManager=GameObject.FindObjectsByType(typeof(InventoryManager),FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<InventoryManager>().First();
        Debug.Log("Find:" + _inventoryManager);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            bool added = _inventoryManager.AddItemInInventory(_itemName, _quantity,_sprite,_itemDescription);
            if(added)
            {
                Destroy(gameObject);
            }
            
        }
    }
}
