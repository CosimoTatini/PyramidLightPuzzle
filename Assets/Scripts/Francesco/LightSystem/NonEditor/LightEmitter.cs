using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Rendering.Universal;
using System;

[DisallowMultipleComponent]
public class LightEmitter : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    [SerializeField] private Color _baseColor = Color.gray1;
    public Light2D Light => _light;

    [Header("Dusts quantities")]
    [SerializeField, Min(0)] private int _redAmount = 0;
    [SerializeField, Min(0)] private int _greenAmount = 0;
    [SerializeField, Min(0)] private int _blueAmount = 0;
    [SerializeField, Min(1)] private int _maxAmount = 4;

    public event Action<LightEmitter> OnLightChanged;

    public int RedAmount
    {
        get { return _redAmount; }
        set
        {
            int previousValue = _redAmount;
            if (value < 0) value = 0;
            if (value > _maxAmount) value = _maxAmount;
            _redAmount = value;
            if (previousValue != _redAmount)
            {
                UpdateLight();
            }
        }
    }

    public int GreenAmount
    {
        get { return _greenAmount; }
        set
        {
            int previousValue = _greenAmount;
            if (value < 0) value = 0;
            if (value > _maxAmount) value = _maxAmount;
            _greenAmount = value;
            if (previousValue != _greenAmount)
            {
                UpdateLight();
            }
        }
    }

    public int BlueAmount
    {
        get { return _blueAmount; }
        set
        {
            int previousValue = _blueAmount;
            if (value < 0) value = 0;
            if (value > _maxAmount) value = _maxAmount;
            _blueAmount = value;
            if (previousValue != _blueAmount)
            {
                UpdateLight();
            }
        }
    }

    public Vector3Int RgbAmounts => new(_redAmount, _greenAmount, _blueAmount);

    public int MaxAmount
    {
        get { return _maxAmount; }
        set
        {
            int previousValue = _maxAmount;
            if (value < 1) value = 1;
            _maxAmount = value;
            if (previousValue != _maxAmount)
            {
                UpdateLight();
            }
        }
    }


#if UNITY_EDITOR
    private int _previousRedAmount;
    private int _previousGreenAmount;
    private int _previousBlueAmount;

    private void OnValidate()
    {
        if (_redAmount < 0) _redAmount = 0;
        if (_redAmount > _maxAmount) _redAmount = _maxAmount;
        if (_greenAmount < 0) _greenAmount = 0;
        if (_greenAmount > _maxAmount) _greenAmount = _maxAmount;
        if (_blueAmount < 0) _blueAmount = 0;
        if (_blueAmount > _maxAmount) _blueAmount = _maxAmount;

        if (_previousRedAmount != _redAmount || _previousGreenAmount != _greenAmount || _previousBlueAmount != _blueAmount)
        {
            _previousRedAmount = _redAmount;
            _previousGreenAmount = _greenAmount;
            _previousBlueAmount = _blueAmount;
            UpdateLight();
        }
    }

#endif


    private void Awake()
    {
        if (!_light)
        {
            if (TryGetComponent(out Light2D light))
            {
                _light = light;
            }
            else
            {
                Debug.LogError($"LightEmitter on {gameObject.name} requires a Light2D component.");
            }
        }
    }

    private void UpdateLight()
    {
        if (!_light) return;

        if (_redAmount == 0 && _greenAmount == 0 && _blueAmount == 0)
        {
            _light.color = _baseColor;
            OnLightChanged?.Invoke(this);
            return;
        }

        float r = Mathf.Clamp01((float)_redAmount / _maxAmount);
        float g = Mathf.Clamp01((float)_greenAmount / _maxAmount);
        float b = Mathf.Clamp01((float)_blueAmount / _maxAmount);
        _light.color = new Color(r, g, b);
        OnLightChanged?.Invoke(this);
    }


}
