using System;
using Unity.Cinemachine;
using UnityEngine;


public class CameraZoomController : MonoBehaviour
{

    [Header("Zoom Settings")]
    [SerializeField] private float _minZoom = 4f;
    [SerializeField] private float _maxZoom = 10f;
    [SerializeField] private float _zoomStep = 1f;

    [Header("Fluidity Settings")]
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _targetTolerance = 0.01f;

    private CinemachineCamera _camera;
    private bool _isZooming;
    private float _targetZoom;


    private void Awake()
    {
        _camera = GetComponent<CinemachineCamera>();
        _targetZoom = _camera.Lens.OrthographicSize;
    }


    public void ZoomIn()
    {
       SetNewTarget(-_zoomStep);
    }

    private void SetNewTarget(float amount)
    {
        _targetZoom=Mathf.Clamp(_targetZoom+amount,_minZoom,_maxZoom);
        _isZooming=true;
    }
    public void ZoomOut()
    { 
      SetNewTarget(_zoomStep);
    }

    private void Update()
    {
        if (!_isZooming) return;

        float currentSize= _camera.Lens.OrthographicSize;

        float newSize = Mathf.MoveTowards(currentSize, _targetZoom, _zoomStep * Time.deltaTime);
        _camera.Lens.OrthographicSize = newSize;

        if(Mathf.Abs(newSize-_targetTolerance)<_targetTolerance)
        {
            _camera.Lens.OrthographicSize = _targetZoom;
            _isZooming=false;
        }
    }
}
