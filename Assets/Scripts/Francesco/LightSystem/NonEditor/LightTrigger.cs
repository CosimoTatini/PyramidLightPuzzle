using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lets the user light this gameObject to interact with another one
/// </summary>
[RequireComponent(typeof(LightSensor))]
[DefaultExecutionOrder(-10)]
public class LightTrigger : MonoBehaviour
{
    [Header("Radius")]
    [Tooltip("The radius in which the light trigger can communicate with subscribers")]
    [Min(0.01f)]
    [SerializeField] private float _activationRadius = 10f;
    [SerializeField] private CircleCollider2D _detectionTrigger;

    public float ActivationRadius
    {
        get
        {
            return _activationRadius;
        }
        set
        {

            if (value >= 0.01f)
                _activationRadius = value;
            if (_detectionTrigger)
            {
                _detectionTrigger.radius = _activationRadius;
            }
        }
    }

    [Header("Subscribers")]
    [SerializeField] private List<IR_ILightTriggerReceiver> _receivers;

    public bool IsActive => _lightSensor.IsActive;

    // these 3 events are private to avoid unintentional subscriptions, you can add listeners using public methods, also we don't need to pass LightSensor, we can just Get it with a public => LightSensor
    private UnityEvent<LightTrigger> _onLightActivated = new();
    private UnityEvent<LightTrigger> _onLightChanged = new();
    private UnityEvent<LightTrigger> _onLightDeactivated = new();

    private LightSensor _lightSensor;
    public LightSensor LightSensor => _lightSensor;

    private void Awake()
    {
        _lightSensor = GetComponent<LightSensor>();
        for (int i = 0; i < _receivers.Count; i++)
        {
            foreach (var lightTriggerReceiver in _receivers[i].Value)
            {
                lightTriggerReceiver.SetLightTrigger(this);
            }
        }
#if UNITY_EDITOR
        _previousActivationRadius = _activationRadius;
#endif
        _detectionTrigger.radius = _activationRadius;
    }

    private void OnEnable()
    {
        _lightSensor.OnLightActivated.AddListener(InvokeOnLightActivated);
        _lightSensor.OnLightChanged.AddListener(InvokeOnLightChanged);
        _lightSensor.OnLightDeactivated.AddListener(InvokeOnLightDeactivated);
    }

    private void OnDisable()
    {
        _lightSensor.OnLightActivated.RemoveListener(InvokeOnLightActivated);
        _lightSensor.OnLightChanged.RemoveListener(InvokeOnLightChanged);
        _lightSensor.OnLightDeactivated.RemoveListener(InvokeOnLightDeactivated);
    }

#if UNITY_EDITOR
    private float _previousActivationRadius;
    private void OnValidate()
    {
        if (_previousActivationRadius != _activationRadius)
        {
            ActivationRadius = _activationRadius;
            _previousActivationRadius = _activationRadius;
        }
    }
#endif

    private void InvokeOnLightActivated()
    {
        _onLightActivated.Invoke(this);
        for (int i = 0; i < _receivers.Count; i++)
        {
            foreach (var lightTriggerReceiver in _receivers[i].Value)
            {
                lightTriggerReceiver.LightActivated();
            }
        }
    }
    private void InvokeOnLightChanged()
    {
        _onLightChanged.Invoke(this);
        for (int i = 0; i < _receivers.Count; i++)
        {
            foreach (var lightTriggerReceiver in _receivers[i].Value)
            {
                lightTriggerReceiver.LightChanged();
            }
        }
    }

    private void InvokeOnLightDeactivated()
    {
        _onLightDeactivated.Invoke(this);
        for (int i = 0; i < _receivers.Count; i++)
        {
            foreach (var lightTriggerReceiver in _receivers[i].Value)
            {
                lightTriggerReceiver.LightDeactivated();
            }
        }
    }

    /* OLD RUNTIME CODE TO ADD/REMOVE LISTENERS
     * 
     * 
     * 
    [SerializeField] private bool _useRadius = true;
    [SerializeField] private float _radius = 10f;
    [Header("Filter")]
    [SerializeField] private bool _useFilter;
    [SerializeField] private List<TypeFilter> _filterTypes;
    // keep track of all components subscribed at runtime to the different UnityEvents
    private List<MonoBehaviour> _runtimeOnLightActivatedTriggerReceivers = new();
    private List<MonoBehaviour> _runtimeOnLightChangedTriggerReceivers = new();
    private List<MonoBehaviour> _runtimeOnLightDeactivatedTriggerReceivers = new();
    // --- OnLightActivated ---
    public void AddOnLightActivatedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        if (!CanBeAdded(subscriber)) return;
        _onLightActivated.AddListener(action);
        if (!_runtimeOnLightActivatedTriggerReceivers.Contains(subscriber))
        {
            _runtimeOnLightActivatedTriggerReceivers.Add(subscriber);
        }
    }
    public void RemoveOnLightActivatedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        _onLightActivated.RemoveListener(action);
        _runtimeOnLightActivatedTriggerReceivers.Remove(subscriber);
    }

    // --- OnLightChanged ---
    public void AddOnLightChangedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        if (!CanBeAdded(subscriber)) return;
        _onLightChanged.AddListener(action);
        if (!_runtimeOnLightChangedTriggerReceivers.Contains(subscriber))
        {
            _runtimeOnLightChangedTriggerReceivers.Add(subscriber);
        }
    }

    public void RemoveOnLightChangedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        _onLightChanged.RemoveListener(action);
        _runtimeOnLightChangedTriggerReceivers.Remove(subscriber);
    }

    // --- OnLightDeactivated ---
    public void AddOnLightDeactivatedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        if (!CanBeAdded(subscriber)) return;
        _onLightDeactivated.AddListener(action);
        if (!_runtimeOnLightDeactivatedTriggerReceivers.Contains(subscriber))
        {
            _runtimeOnLightDeactivatedTriggerReceivers.Add(subscriber);
        }
    }

    public void RemoveOnLightDeactivatedListener(MonoBehaviour subscriber, UnityAction<LightTrigger> action)
    {
        _onLightDeactivated.RemoveListener(action);
        _runtimeOnLightDeactivatedTriggerReceivers.Remove(subscriber);
    }
    private bool CanBeAdded(MonoBehaviour behavior)
    {
        if (behavior == null) return false;
        if (behavior.gameObject == null) return false;
        if (_useRadius)
        {
            Debug.Log(Vector2.Distance(behavior.gameObject.transform.position, transform.position));
            if (Vector2.Distance(behavior.gameObject.transform.position, transform.position) > _radius) return false;
        }
        if (_useFilter)
        {
            bool foundMatch = false;
            Type behaviorType = behavior.GetType();
            foreach (var typeFilter in _filterTypes)
            {
                if (typeFilter.TypeVar == null) continue;

                Type type = typeFilter.TypeVar.Type;
                if (type == null) continue;

                if (typeFilter.StrictMode)
                {
                    if (type == behaviorType)
                    {
                        foundMatch = true;
                        break;
                    }
                }
                else
                {
                    if (type.IsAssignableFrom(behaviorType))
                    {
                        foundMatch = true;
                        break;
                    }
                }
            }

            if (!foundMatch) return false;
        }
        return true;
    }
    */
}
