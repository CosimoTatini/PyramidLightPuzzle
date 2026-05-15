using System;
using UnityEngine;

public class ThreeWayRGBSplit : MonoBehaviour
{
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
