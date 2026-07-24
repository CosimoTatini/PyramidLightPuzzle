using UnityEngine;

public class BeamConverter : MonoBehaviour
{
    [SerializeField] private LightSensor _lightSensor;
    [SerializeField] private LightEmitter _lightEmitter;

    void OnEnable()
    {
        if (_lightSensor == null || _lightEmitter == null) return;
        _lightSensor.OnLightChanged.RemoveListener(UpdateLightEmitter);
        _lightSensor.OnLightChanged.AddListener(UpdateLightEmitter);
    }

    void OnDisable()
    {
        if (_lightSensor == null || _lightEmitter == null) return;
        _lightSensor.OnLightChanged.RemoveListener(UpdateLightEmitter);
    }

    private void UpdateLightEmitter()
    {
        if (_lightSensor == null || _lightEmitter == null) return;
        _lightEmitter.CanUpdateLight = false;
        _lightEmitter.RedAmount = _lightSensor.CurrentRedAmount;
        _lightEmitter.GreenAmount = _lightSensor.CurrentGreenAmount;
        _lightEmitter.CanUpdateLight = true;
        _lightEmitter.BlueAmount = _lightSensor.CurrentBlueAmount;
    }
}
