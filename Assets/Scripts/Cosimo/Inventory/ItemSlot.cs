using JetBrains.Annotations;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Data")]
    [SerializeField] private string _itemName;
    [SerializeField] private int _quantity;
    [SerializeField] private Sprite _sprite;
    public bool IsFull;
    public string ItemDescription;

    [Header("Item Slot")]
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _itemImage;

    [Header("Item Description")]
    [SerializeField] private Image _itemImageDescription;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;
    
    
    public GameObject SelectedShader;
    public bool IsSelected;
    private InventoryManager _inventoryManager;

    private void Start()
    {
        _inventoryManager= GameObject.FindObjectsByType(typeof(InventoryManager), FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<InventoryManager>().First();
    }

    public void AddItemInSlot(string itemName, int quantity, Sprite sprite, string description)
    {
        this._itemName= itemName;
        this._quantity = quantity;
        this._sprite = sprite;
        this.ItemDescription = description;
        IsFull = true;

        _quantityText.text = _quantity.ToString();
        _quantityText.enabled = true;
        _itemImage.sprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       if(eventData.button== PointerEventData.InputButton.Left)
       {
            OnLeftClick();
       }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }
    private void OnLeftClick()
    {
        _inventoryManager.DeselectAllSlots();
        SelectedShader.SetActive(true);
        IsSelected = true;
        _itemNameText.text = _itemName;
        _itemDescriptionText.text = ItemDescription;
        _itemImageDescription.sprite = _sprite;
    }

    private void OnRightClick()
    {
        _inventoryManager.DeselectAllSlots();
    }

  
}
