using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lets the user light this gameObject to interact with another one
/// </summary>
[RequireComponent(typeof(LightSensor))]
public class LightTrigger : MonoBehaviour
{
    /* VARIABLES
        - LightSensor _lightSensor
        - float _radius
        - bool _useRadius // can't be changed during runtime to avoid discrepancies
        - List<MonoBehaviour> _runtimeOnLightActivatedTriggerReceivers 
        - List<MonoBehaviour> _runtimeOnLightChangedTriggerReceivers 
        - List<MonoBehaviour> _runtimeOnLightDeactivatedTriggerReceivers 
        - List<TypeSO> _filterType // filters trigger receivers based on type
        - bool _useFilter
        - UnityEvent _onLightActivated // these 3 events are private to avoid unintentional subscriptions, you can add listeners using public methods, also we don't need to pass LightSensor, we can just Get it with a public => LightSensor
        - UnityEvent _onLightChanged
        - UnityEvent _onLightDeactivated
       METHODS
        - public void AddOnLightActivatedListener(UnityAction) // adds an action (needs to be UnityAction, doesn't really change anything) to subscribe
                                                               // we need to check if in radius (if toggled) and if part of the filter (if toggled) 
        - ..................................................... // Same for the other 2 events
        - public void RemoveOnLightActivatedListener(UnityAction) // removes an action
                                                                  // we need to check if inside the List of _runtimeTriggerReceivers 
        - ..................................................... // Same for the other 2 events
        - private void OnLightActivated() // "repeats" the signal from the light Sensor to the subscribers of _onLightActivated
        - private void OnLightChanged() // "repeats" the signal from the light Sensor to the subscribers of _onLightChanged
        - private void OnLightDeactivated() // "repeats" the signal from the light Sensor to the subscribers of _onLightDeactivated
    */

    [Header("Radius")]
    [Tooltip("The radius in which the light trigger can communicate with subscribers")]
    [SerializeField] private bool _useRadius = true;
    [SerializeField] private float _radius = 10f;

    [Header("Filter")]
    [SerializeField] private bool _useFilter;
    [SerializeField] private List<TypeFilter> _filterTypes;

    [Header("Subscribers")]
    [SerializeField] private List<InterfaceReferenceILightTriggerReceiver> _receivers;

    //TODO: update the status and make it accessible to others
    private bool _isActive = false;

    // keep track of all components subscribed at runtime to the different UnityEvents
    private List<MonoBehaviour> _runtimeOnLightActivatedTriggerReceivers = new();
    private List<MonoBehaviour> _runtimeOnLightChangedTriggerReceivers = new();
    private List<MonoBehaviour> _runtimeOnLightDeactivatedTriggerReceivers = new();

    // these 3 events are private to avoid unintentional subscriptions, you can add listeners using public methods, also we don't need to pass LightSensor, we can just Get it with a public => LightSensor
    private UnityEvent<LightTrigger> _onLightActivated = new();
    private UnityEvent<LightTrigger> _onLightChanged = new();
    private UnityEvent<LightTrigger> _onLightDeactivated = new();

    private LightSensor _lightSensor;
    public LightSensor LightSensor => _lightSensor;

    private void Awake()
    {
        _lightSensor = GetComponent<LightSensor>();
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

    private void InvokeOnLightActivated()
    {
        Debug.Log("Activated");
        _onLightActivated.Invoke(this);
    }
    private void InvokeOnLightChanged()
    { 
        _onLightChanged.Invoke(this);
    }

    private void InvokeOnLightDeactivated()
    { 
        _onLightDeactivated.Invoke(this);
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    //TODO: Maybe check radius with trigger exit, this would require saving for each MonoBehavior the connected UnityActions, so 1 dictionary for each type (activated, changed, deactivated)
    // and would be <MonoBehavior, List<UnityAction<LightTrigger>>> since a component could subscribe with different methods to the same UnityEvent
    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}
