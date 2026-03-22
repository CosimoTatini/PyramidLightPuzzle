using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LightSensor : MonoBehaviour, ILightReceiver
{
    [SerializeField] private bool _isActive = false;
    public bool IsActive
    {
        get { return _isActive; }
        private set
        {
            if (_isActive != value)
            {
                _isActive = value;
                if (_isActive)
                {
                    OnLightActivated.Invoke();
                }
                else
                {
                    OnLightDeactivated.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// keeps track of currently in range emitters and their contribute to the amounts, also useful when an emitter changes so we know what its contribute was
    /// </summary>
    private Dictionary<LightEmitter, Vector3Int> _emittersInRangeAndValues = new();

    [Header("Events")]
    public UnityEvent OnLightActivated;
    public UnityEvent OnLightChanged;
    public UnityEvent OnLightDeactivated;

    [Header("Dusts quantities")]
    [SerializeField, Min(0)] private int _neededRedAmount = 1;
    [SerializeField, Min(0)] private int _neededGreenAmount = 1;
    [SerializeField, Min(0)] private int _neededBlueAmount = 1;

    public int NeededRedAmount
    {
        get { return _neededRedAmount; }
    }

    public int NeededGreenAmount
    {
        get { return _neededGreenAmount; }
    }

    public int NeededBlueAmount
    {
        get { return _neededBlueAmount; }
    }
    public Vector3Int NeededRgbAmounts => new(_neededRedAmount, _neededGreenAmount, _neededBlueAmount);

    public int CurrentRedAmount
    {
        get
        {
            return _emittersInRangeAndValues.Sum(kvp => kvp.Value.x);
        }
    }
    public int CurrentGreenAmount
    {
        get
        {
            return _emittersInRangeAndValues.Sum(kvp => kvp.Value.y);
        }
    }
    public int CurrentBlueAmount
    {
        get
        {
            return _emittersInRangeAndValues.Sum(kvp => kvp.Value.z);
        }
    }

    public Vector3Int CurrentRgbAmounts => new(CurrentRedAmount, CurrentGreenAmount, CurrentBlueAmount);

#if UNITY_EDITOR

    private int _previousNeededRedAmount;
    private int _previousNeededGreenAmount;
    private int _previousNeededBlueAmount;

    private void OnValidate()
    {
        // update the when the color requirements change in the editor
        if (_previousNeededRedAmount != _neededRedAmount || _previousNeededGreenAmount != _neededGreenAmount || _previousNeededBlueAmount != _neededBlueAmount)
        {
            _previousNeededRedAmount = _neededRedAmount;
            _previousNeededGreenAmount = _neededGreenAmount;
            _previousNeededBlueAmount = _neededBlueAmount;

            OnLightChanged?.Invoke();
            IsActive = AreAmountsRight();
        }
    }
#endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightEmitter emitter))
        {
            Debug.Log("Emitter entered: " + collision.name);
            AddLight(emitter);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightEmitter emitter))
        {
            Debug.Log("Emitter exited: " + collision.name);
            RemoveLight(emitter);
        }
    }

    public void AddLight(LightEmitter emitter)
    {
        if (_emittersInRangeAndValues.ContainsKey(emitter)) return;

        _emittersInRangeAndValues[emitter] = emitter.RgbAmounts;
        if (emitter.RgbAmounts != Vector3Int.zero)
        {
            OnLightChanged.Invoke();
        }

        IsActive = AreAmountsRight();

        emitter.OnLightChanged += OnEmitterChanged;
    }

    public void RemoveLight(LightEmitter emitter)
    {
        if (!_emittersInRangeAndValues.ContainsKey(emitter)) return;

        _emittersInRangeAndValues.Remove(emitter);
        if (emitter.RgbAmounts != Vector3Int.zero)
        {
            OnLightChanged.Invoke();
        }

        IsActive = AreAmountsRight();
        emitter.OnLightChanged -= OnEmitterChanged;
    }

    /// <summary>
    /// Tells if the amounts are exacat to activate the sensor, if the emitter has more or less of one of the colors, the sensor won't be activated
    /// </summary>
    /// <returns>True if there are the exact amounts, fale otherwise</returns>
    public bool AreAmountsRight()
    {
        if (NeededRgbAmounts == CurrentRgbAmounts)
        {
            return true;
        }
        return false;
    }

    private void OnEmitterChanged(LightEmitter emitter)
    {
        _emittersInRangeAndValues[emitter] = emitter.RgbAmounts;
        OnLightChanged.Invoke();

        IsActive = AreAmountsRight();
    }
}
