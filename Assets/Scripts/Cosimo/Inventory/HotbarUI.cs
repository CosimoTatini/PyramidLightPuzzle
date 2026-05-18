using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private Image[] _itemIcons;
    [SerializeField] private TextMeshProUGUI[] _quantityTexts;
    [SerializeField] private GameObject[] _selectionHighlights;

    private void Update()
    {
        {
            if (_inventoryManager == null) return;

            // Recuperiamo il RIFERIMENTO all'oggetto selezionato, non l'indice
            var selectedItem = _inventoryManager.GetSelectedItem();

            for (int i = 0; i < _inventoryManager.Items.Count; i++)
            {
                var currentItem = _inventoryManager.Items[i];

                // Aggiorniamo i testi e le icone usando i dati dell'item
                _quantityTexts[i].text = currentItem.Quantity.ToString();
                _itemIcons[i].sprite = currentItem.Sprite;

                // CONFRONTO TRA OGGETTI: Verifichiamo se l'item di questo slot 
                // è lo stesso oggetto selezionato nel manager
                bool isSelected = (currentItem == selectedItem);
                _selectionHighlights[i].SetActive(isSelected);
            }
        }
    }
}

