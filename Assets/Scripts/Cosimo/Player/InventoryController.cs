using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Opening and closing of Inventory
/// TODO: Aggiungere qui funzione di navigazione dell'inventario
/// TODO: Anzichè fermare il tempo, abilita l'action map della UI e disabilita quella del Player e viceversa  
/// </summary>
public class InventoryController : MonoBehaviour
{
    private InputSystem_Actions _inputActions;

    [SerializeField] GameObject _inventoryPanel;

    [SerializeField] bool _isActive;


    private void Awake()
    {
        _inventoryPanel.SetActive(false);
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.OpenMenu.performed += OnOpenMenuPerformed;
    }

    private void OnDisable()
    {
        _inputActions.UI.OpenMenu.performed -= OnOpenMenuPerformed;
        _inputActions.Disable();
    }


    private void OnOpenMenuPerformed(InputAction.CallbackContext context)
    {
        _isActive = !_inventoryPanel.activeSelf;
        _inventoryPanel.SetActive(_isActive);

        if(_isActive )
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;

        }
    }
}

    
