using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeWayRGBSplit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _diamondRenderer;
    [Header("Settings")]
    [SerializeField] private float _rotationTime = 1f;
    [SerializeField] private AnimationCurve _rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private float _maxRaycastDistance = 999f;

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

    private bool _isRotating = false;
    public bool IsRotating => _isRotating;
    private float _rotationAngle = 90f;
    public event Action OnRotationStarted;
    public event Action OnRotationCompleted;

    private void Awake()
    {
        SetEmittersToZero();
        ToggleColliders(false);
        InitializeEmittersFreeFormLengthIndexes();
    }

    private void ToggleColliders(bool toggle)
    {
        _redEmitterCollider.enabled = toggle;
        _greenEmitterCollider.enabled = toggle;
        _blueEmitterCollider.enabled = toggle;
    }


    private void OnEnable()
    {
        _lightSensor.OnLightChanged.AddListener(LightChanged);
    }

    private void OnDisable()
    {
        _lightSensor.OnLightChanged.RemoveListener(LightChanged);
    }
    private void InitializeEmittersFreeFormLengthIndexes()
    {
        _emittersFreeFormLengthIndexes.Add(_redEmitter, new(_redEmitter.Light.shapePath.Length - 1, _redEmitter.Light.shapePath.Length - 2));
        _emittersFreeFormLengthIndexes.Add(_greenEmitter, new(_greenEmitter.Light.shapePath.Length - 1, _greenEmitter.Light.shapePath.Length - 2));
        _emittersFreeFormLengthIndexes.Add(_blueEmitter, new(_blueEmitter.Light.shapePath.Length - 1, _blueEmitter.Light.shapePath.Length - 2));
    }

    [ContextMenu("Rotate")]
    public void Rotate()
    {
        if (_isRotating) return;
        _isRotating = true;
        OnRotationStarted?.Invoke();
        SetEmittersToZero();
        ToggleColliders(false);
        StartCoroutine(RotationCoroutine());
    }

    private IEnumerator RotationCoroutine()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, -_rotationAngle);

        float elapsedTime = 0f;

        while (elapsedTime < _rotationTime)
        {
            elapsedTime += Time.deltaTime;
            float percentage = Mathf.Clamp01(elapsedTime / _rotationTime);

            float curvePercentage = _rotationCurve.Evaluate(percentage);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curvePercentage);
            yield return null;
        }
        transform.rotation = targetRotation;

        // update the light after rotating in case something changed
        //if (_didLightChangeWhileRotating)
        //{ 
        //    _didLightChangeWhileRotating = false;
        //    LightChanged();
        //}
        // allow for a new rotation
        ToggleColliders(true);
        _isRotating = false;
        OnRotationCompleted?.Invoke();
        LightChanged();
    }

    private void LightChanged()
    {
        if (_isRotating)
        {
            return;
        }
        SetDusts();
        RaycastAndLightExpansion();
        UpdateDiamondColor();
    }

    private void UpdateDiamondColor()
    {
        float r = Mathf.Clamp01((float)_lightSensor.CurrentRedAmount / _lightSensor.MaxAmount);
        float g = Mathf.Clamp01((float)_lightSensor.CurrentGreenAmount / _lightSensor.MaxAmount);
        float b = Mathf.Clamp01((float)_lightSensor.CurrentBlueAmount / _lightSensor.MaxAmount);

        _diamondRenderer.color = new Color(r, g, b, 1f);
    }

    private void RaycastAndLightExpansion()
    {
        // raycast from the emitters up direction if the emitters have at least 1 dust

        if (_redEmitter.RedAmount > 0)
        {
            if (_redEmitterCollider.enabled == false) _redEmitterCollider.enabled = true;
            RaycastHit2D redCast = Physics2D.Raycast(_redEmitterPivot.position, _redEmitterPivot.up, _maxRaycastDistance, _hitMask);
            if (redCast.collider)
            {
                Debug.Log("RedCollider " + redCast.collider);
                // Vector3 worldOffset = redCast.point - (Vector2)_redEmitter.Light.transform.position;
                // float localDistance = Vector3.Dot(worldOffset, _redEmitter.Light.transform.up);

                // set new path with updated length for the freeform light
                Vector3[] newPath = _redEmitter.Light.shapePath;
                // newPath[_emittersFreeFormLengthIndexes[_redEmitter].x].y = localDistance;
                // newPath[_emittersFreeFormLengthIndexes[_redEmitter].y].y = localDistance;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].x].y = _redEmitter.Light.transform.InverseTransformPoint(redCast.point).y;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].y].y = _redEmitter.Light.transform.InverseTransformPoint(redCast.point).y;

                _redEmitter.Light.SetShapePath(newPath);
                // _redEmitterCollider.size = new(_redEmitterCollider.size.x, localDistance + 0.1f);
                _redEmitterCollider.size = new(_redEmitterCollider.size.x, redCast.distance + 0.1f);
                _redEmitterCollider.offset = new(_redEmitterCollider.offset.x, _redEmitterCollider.size.y / 2);
            }
            else
            {
                Vector3[] newPath = _redEmitter.Light.shapePath;
                Vector3 maxWorldPoint = _redEmitterPivot.position + _redEmitterPivot.up * _maxRaycastDistance;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].x].y = _redEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;
                newPath[_emittersFreeFormLengthIndexes[_redEmitter].y].y = _redEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;

                _redEmitter.Light.SetShapePath(newPath);
                _redEmitterCollider.size = new(_redEmitterCollider.size.x, _maxRaycastDistance + 0.1f);
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
            RaycastHit2D greenCast = Physics2D.Raycast(_greenEmitterPivot.position, _greenEmitterPivot.up, _maxRaycastDistance, _hitMask);
            if (greenCast.collider)
            {
                Debug.Log("GreenCollider " + greenCast.collider);
                Vector3 worldOffset = greenCast.point - (Vector2)_greenEmitter.Light.transform.position;
                float localDistance = Vector3.Dot(worldOffset, _greenEmitter.Light.transform.up);

                // set new path with updated length for the freeform light
                Vector3[] newPath = _greenEmitter.Light.shapePath;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].x].y = localDistance;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].y].y = localDistance;
                // newPath[_emittersFreeFormLengthIndexes[_greenEmitter].x].y = _greenEmitter.Light.transform.InverseTransformPoint(greenCast.point).y;
                // newPath[_emittersFreeFormLengthIndexes[_greenEmitter].y].y = _greenEmitter.Light.transform.InverseTransformPoint(greenCast.point).y;

                _greenEmitter.Light.SetShapePath(newPath);
                _greenEmitterCollider.size = new(_greenEmitterCollider.size.x, localDistance + 0.1f);
                // _greenEmitterCollider.size = new(_greenEmitterCollider.size.x, greenCast.distance + 0.1f);
                _greenEmitterCollider.offset = new(_greenEmitterCollider.offset.x, _greenEmitterCollider.size.y / 2);
            }
            else
            {
                Vector3[] newPath = _greenEmitter.Light.shapePath;
                Vector3 maxWorldPoint = _greenEmitterPivot.position + _greenEmitterPivot.up * _maxRaycastDistance;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].x].y = _greenEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;
                newPath[_emittersFreeFormLengthIndexes[_greenEmitter].y].y = _greenEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;

                _greenEmitter.Light.SetShapePath(newPath);
                _greenEmitterCollider.size = new(_greenEmitterCollider.size.x, _maxRaycastDistance + 0.1f);
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
            RaycastHit2D blueCast = Physics2D.Raycast(_blueEmitterPivot.position, _blueEmitterPivot.up, _maxRaycastDistance, _hitMask);
            if (blueCast.collider)
            {
                Debug.Log("BlueCollider " + blueCast.collider);
                Vector3 worldOffset = blueCast.point - (Vector2)_blueEmitter.Light.transform.position;
                float localDistance = Vector3.Dot(worldOffset, _blueEmitter.Light.transform.up);
                // set new path with updated length for the freeform light
                Vector3[] newPath = _blueEmitter.Light.shapePath;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].x].y = localDistance;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].y].y = localDistance;
                // newPath[_emittersFreeFormLengthIndexes[_blueEmitter].x].y = _blueEmitter.Light.transform.InverseTransformPoint(blueCast.point).y;
                // newPath[_emittersFreeFormLengthIndexes[_blueEmitter].y].y = _blueEmitter.Light.transform.InverseTransformPoint(blueCast.point).y;

                _blueEmitter.Light.SetShapePath(newPath);
                _blueEmitterCollider.size = new(_blueEmitterCollider.size.x, localDistance + 0.1f);
                // _blueEmitterCollider.size = new(_blueEmitterCollider.size.x, blueCast.distance + 0.1f);
                _blueEmitterCollider.offset = new(_blueEmitterCollider.offset.x, _blueEmitterCollider.size.y / 2);
            }
            else
            {
                Vector3[] newPath = _blueEmitter.Light.shapePath;
                Vector3 maxWorldPoint = _blueEmitterPivot.position + _blueEmitterPivot.up * _maxRaycastDistance;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].x].y = _blueEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;
                newPath[_emittersFreeFormLengthIndexes[_blueEmitter].y].y = _blueEmitter.Light.transform.InverseTransformPoint(maxWorldPoint).y;

                _blueEmitter.Light.SetShapePath(newPath);
                _blueEmitterCollider.size = new(_blueEmitterCollider.size.x, _maxRaycastDistance + 0.1f);
                _blueEmitterCollider.offset = new(_blueEmitterCollider.offset.x, _blueEmitterCollider.size.y / 2);
            }
        }
        else
        {
            if (_blueEmitterCollider.enabled == true) _blueEmitterCollider.enabled = false;
        }
    }

    private void SetDusts()
    {
        // avoid updating the light twice for each emitter
        _redEmitter.CanUpdateLight = false;
        _greenEmitter.CanUpdateLight = false;
        _blueEmitter.CanUpdateLight = false;

        _redEmitter.MaxAmount = _lightSensor.MaxAmount;
        _greenEmitter.MaxAmount = _lightSensor.MaxAmount;
        _blueEmitter.MaxAmount = _lightSensor.MaxAmount;

        _redEmitter.CanUpdateLight = true;
        _greenEmitter.CanUpdateLight = true;
        _blueEmitter.CanUpdateLight = true;

        _redEmitter.RedAmount = _lightSensor.CurrentRedAmount;
        _greenEmitter.GreenAmount = _lightSensor.CurrentGreenAmount;
        _blueEmitter.BlueAmount = _lightSensor.CurrentBlueAmount;
    }

    private void SetEmittersToZero()
    {
        _redEmitter.SetToZero();
        _greenEmitter.SetToZero();
        _blueEmitter.SetToZero();
    }

}
