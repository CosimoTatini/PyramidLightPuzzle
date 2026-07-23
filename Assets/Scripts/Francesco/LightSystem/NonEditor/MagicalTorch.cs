using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
using UnityEngine.U2D;

public class MagicalTorch : MonoBehaviour
{
    [SerializeField] private Light2DBase _light;

    private void Awake()
    {
        if (!_light)
        {
            if (TryGetComponent(out Light2DBase light))
            {
                _light = light;
            }
            else
            {
                Debug.LogError($"Torch on {gameObject.name} requires a Light2D component.");
            }
        }
    }

    void Start()
    {
        InventoryManager.Instance.UseTorch();
    }

    void OnDestroy()
    {
        InventoryManager.Instance.ReturnTorch(TorchType.Magical);
        if (TryGetComponent(out LightEmitter lightEmitter))
        {
            if (lightEmitter.RedAmount > 0)
            {
                InventoryManager.Instance.AddPowder(PowderColor.Red, lightEmitter.RedAmount);
            }

            if (lightEmitter.GreenAmount > 0)
            {
                InventoryManager.Instance.AddPowder(PowderColor.Green, lightEmitter.GreenAmount);
            }

            if (lightEmitter.BlueAmount > 0)
            {
                InventoryManager.Instance.AddPowder(PowderColor.Blue, lightEmitter.BlueAmount);
            }
        }
    }
}
