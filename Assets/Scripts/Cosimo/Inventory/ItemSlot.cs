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

    [Header("Item Slot")]
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _itemImage;
    
    
    public GameObject SelectedShader;
    public bool IsSelected;
    private InventoryManager _inventoryManager;

    private void Start()
    {
        _inventoryManager= GameObject.FindObjectsByType(typeof(InventoryManager), FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<InventoryManager>().First();
    }

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
    }

    private void OnRightClick()
    {
        _inventoryManager.DeselectAllSlots();
    }

  
}
