using System;
using UnityEngine;

public class ThreeWayRGBSplit : MonoBehaviour
{
    [SerializeField] private LightSensor _lightSensor;
    [Header("Light Emitters")]
    [SerializeField] private LightEmitter _redEmitter;
    [SerializeField] private LightEmitter _greenEmitter;
    [SerializeField] private LightEmitter _blueEmitter;

    private void OnEnable()
    {
        _lightSensor.OnLightActivated.AddListener(LightActivated);
        _lightSensor.OnLightChanged.AddListener(LightChanged);
        _lightSensor.OnLightDeactivated.AddListener(LightDeactivated);
    }

    private void OnDisable()
    {
        _lightSensor.OnLightActivated.RemoveListener(LightActivated);
        _lightSensor.OnLightChanged.RemoveListener(LightChanged);
        _lightSensor.OnLightDeactivated.RemoveListener(LightDeactivated);
    }

    private void LightActivated()
    {

    }

    private void LightDeactivated()
    {
    }

    private void LightChanged()
    {

    }

    private void SetLights()
    { 
        
    }
}
