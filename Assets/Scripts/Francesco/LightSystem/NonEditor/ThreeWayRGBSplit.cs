using System;
using System.Collections.Generic;
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
    [SerializeField] private BoxCollider2D _redEmitterCollider;
    [SerializeField] private BoxCollider2D _greenEmitterCollider;
    [SerializeField] private BoxCollider2D _blueEmitterCollider;

    // saves the indexes we need to modify for the freeform light to be the same length of the raycast hit
    private Dictionary<LightEmitter, Vector2Int> _emittersFreeFormLengthIndexes = new();

    private void Awake()
    {
        SetToZero();
        _emittersFreeFormLengthIndexes.Add(_redEmitter, new(_redEmitter.Light.shapePath.Length - 1, _redEmitter.Light.shapePath.Length - 2));
        _emittersFreeFormLengthIndexes.Add(_greenEmitter, new(_greenEmitter.Light.shapePath.Length - 1, _greenEmitter.Light.shapePath.Length - 2));
        _emittersFreeFormLengthIndexes.Add(_blueEmitter, new(_blueEmitter.Light.shapePath.Length - 1, _blueEmitter.Light.shapePath.Length - 2));
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

        // raycast from the emitters up direction if the emitters have at least 1 dust

        if (_redEmitter.RedAmount > 0)
        {
            if (_redEmitterCollider.enabled == false) _redEmitterCollider.enabled = true;
            RaycastHit2D redCast = Physics2D.Raycast(_redEmitterPivot.position, _redEmitterPivot.up, Mathf.Infinity, _hitMask);
            if (redCast.collider)
            {
                // set new path with updated length for the freeform light
                Vector3[] newPath = _redEmitter.Light.shapePath;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].x].y = _redEmitter.Light.transform.InverseTransformPoint(redCast.point).y;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].y].y = _redEmitter.Light.transform.InverseTransformPoint(redCast.point).y;

                _redEmitter.Light.SetShapePath(newPath);
                _redEmitterCollider.size = new(_redEmitterCollider.size.x, redCast.distance + 0.1f);
                _redEmitterCollider.offset = new(_redEmitterCollider.offset.x, _redEmitterCollider.size.y / 2);
            }
        }
        else
        {
            if (_redEmitterCollider.enabled == true) _redEmitterCollider.enabled = false;
        }

        if (_greenEmitter.GreenAmount > 0)
        {
            if (_greenEmitterCollider.enabled == false) _greenEmitterCollider.enabled = true;
            RaycastHit2D greenCast = Physics2D.Raycast(_greenEmitterPivot.position, _greenEmitterPivot.up, Mathf.Infinity, _hitMask);
            if (greenCast.collider)
            {
                // set new path with updated length for the freeform light
                Vector3[] newPath = _greenEmitter.Light.shapePath;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].x].y = _greenEmitter.Light.transform.InverseTransformPoint(greenCast.point).y;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].y].y = _greenEmitter.Light.transform.InverseTransformPoint(greenCast.point).y;

                _greenEmitter.Light.SetShapePath(newPath);
                _greenEmitterCollider.size = new(_greenEmitterCollider.size.x, greenCast.distance + 0.1f);
                _greenEmitterCollider.offset = new(_greenEmitterCollider.offset.x, _greenEmitterCollider.size.y / 2);
            }
        }
        else
        {
            if (_greenEmitterCollider.enabled == true) _greenEmitterCollider.enabled = false;

        }

        if (_blueEmitter.BlueAmount > 0)
        {
            if (_blueEmitterCollider.enabled == false) _blueEmitterCollider.enabled = true;
            RaycastHit2D blueCast = Physics2D.Raycast(_blueEmitterPivot.position, _blueEmitterPivot.up, Mathf.Infinity, _hitMask);
            if (blueCast.collider)
            {
                // set new path with updated length for the freeform light
                Vector3[] newPath = _blueEmitter.Light.shapePath;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].x].y = _blueEmitter.Light.transform.InverseTransformPoint(blueCast.point).y;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].y].y = _blueEmitter.Light.transform.InverseTransformPoint(blueCast.point).y;

                _blueEmitter.Light.SetShapePath(newPath);
                _blueEmitterCollider.size = new(_blueEmitterCollider.size.x, blueCast.distance + 0.1f);
                _blueEmitterCollider.offset = new(_blueEmitterCollider.offset.x, _blueEmitterCollider.size.y / 2);
            }
        }
        else
        {
            if (_blueEmitterCollider.enabled == true) _blueEmitterCollider.enabled = false;

        }





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
