using UnityEngine;

public class MagicalTorchDisableSwitch : MonoBehaviour
{
    [SerializeField] private InputConfigSO _disableSwitchConfig;
    [SerializeField] private ItemPlacement _itemPlacement;

    private InputSystem_Actions _inputActions;

    void Start()
    {
        if (InputUserEventsManager.Player1.HasValue)
        {
            _inputActions = InputConfigManager.GetInputSytemInstanceGeneric<InputSystem_Actions>(InputUserEventsManager.Player1.Value);
            InputConfigManager.RegisterConfig(_disableSwitchConfig);
            InventoryManager.Instance.ReturnTorch(TorchType.Normal);
        }
    }
    void OnDestroy()
    {
        if (_inputActions != null && !InventoryManager.Instance.MagicalTorchUnlocked)
        {
            InputConfigManager.UnregisterConfig(_disableSwitchConfig);
            InventoryManager.Instance.MagicalTorchUnlocked = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player) && collision.IsTouching(_itemPlacement.Collider2D))
        {
            InputConfigManager.UnregisterConfig(_disableSwitchConfig);
            InventoryManager.Instance.MagicalTorchUnlocked = true;
        }
    }
}
