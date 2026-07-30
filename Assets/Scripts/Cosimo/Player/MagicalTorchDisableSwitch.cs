using UnityEngine;

public class MagicalTorchDisableSwitch : MonoBehaviour
{
    [SerializeField] private InputConfigSO _disableSwitchConfig;

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
        if (_inputActions != null)
        {
            InputConfigManager.UnregisterConfig(_disableSwitchConfig);
            InventoryManager.Instance.MagicalTorchUnlocked = true;
        }
    }
}
