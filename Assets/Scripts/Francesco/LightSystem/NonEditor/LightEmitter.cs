using UnityEngine;
using UnityEngine.U2D;

public class LightEmitter : MonoBehaviour
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
                Debug.LogError($"LightEmitter on {gameObject.name} requires a Light2D component.");
            }
        }
    }
}
