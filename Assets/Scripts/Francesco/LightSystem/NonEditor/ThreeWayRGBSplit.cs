using System;
using UnityEngine;

public class ThreeWayRGBSplit : MonoBehaviour
{
    string diagram = "https://app.diagrams.net/?src=about#LDiagramma%20senza%20titolo.drawio#%7B%22pageId%22%3A%227t_RhQeSFkzK3E7iq9TL%22%7D";
    [Header("Settings")]
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private LayerMask _hitMask;

    [Header("Light References")]
    [SerializeField] private LightSensor _lightSensor;
    [SerializeField] private LightEmitter _redEmitter;
    [SerializeField] private LightEmitter _greenEmitter;
    [SerializeField] private LightEmitter _blueEmitter;
    [SerializeField] private Transform _redEmitterPivot;
    [SerializeField] private Transform _greenEmitterPivot;
    [SerializeField] private Transform _blueEmitterPivot;

    private void Awake()
    {
        SetToZero();
    }

    private void OnEnable()
    {
        _lightSensor.OnLightChanged.AddListener(LightChanged);
    }

    private void OnDisable()
    {
        _lightSensor.OnLightChanged.RemoveListener(LightChanged);
    }

    public void Rotate()
    { 
        
    }

    private void LightChanged()
    {
        SetDusts();
        // boxcast in the forward direction of the emitters
        // if it hits something in the layer, then we have our distance
        // we can use the distance to set both the trigger for the light emitter and the freeform light
        // maybe multiple raycasts would be better in case we want the light not to stop for just a part of the light being blocked, this would still be a problem
        // since then what do we do, reduce the size of the light? that would mean having 2 emitters or more depending on the number of blocking objects
        // so one or more emitters that stop when hit, then 
    }

    private void SetDusts()
    {
        _redEmitter.MaxAmount = _lightSensor.MaxAmount;
        _greenEmitter.MaxAmount = _lightSensor.MaxAmount;
        _blueEmitter.MaxAmount = _lightSensor.MaxAmount;

        _redEmitter.RedAmount = _lightSensor.CurrentRedAmount;
        _greenEmitter.GreenAmount = _lightSensor.CurrentGreenAmount;
        _blueEmitter.BlueAmount = _lightSensor.CurrentBlueAmount;
    }

    private void SetToZero()
    {
        _redEmitter.SetToZero();
        _greenEmitter.SetToZero();
        _blueEmitter.SetToZero();
    }
}
