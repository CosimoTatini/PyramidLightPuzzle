
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string _itemName;
    [SerializeField] private int _quantity;
    [SerializeField] private Sprite _sprite;
    [TextArea]
    [SerializeField] private string _itemDescription;
    public string ItemName => _itemName;
    public int Quantity => _quantity;
    public Sprite Sprite => _sprite;
    public string ItemDescription => _itemDescription;
    private InventoryManager _inventoryManager;

    

    private void Start()
    {
        _inventoryManager=GameObject.FindObjectsByType(typeof(InventoryManager),FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<InventoryManager>().First();
        Debug.Log("Find:" + _inventoryManager);
    }

   
}
