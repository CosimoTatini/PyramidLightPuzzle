using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private string _itemName;
    [SerializeField] private int _quantity;
    [SerializeField] private Sprite _sprite;
    public bool IsFull;

    [Header("Item Slot")]
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _itemImage;

    public void AddItemInSlot(string itemName, int quantity, Sprite sprite)
    {
        this._itemName= itemName;
        this._quantity = quantity;
        this._sprite = sprite;
        IsFull = true;

        _quantityText.text = _quantity.ToString();
        _quantityText.enabled = true;
        _itemImage.sprite = sprite;
    }
}
