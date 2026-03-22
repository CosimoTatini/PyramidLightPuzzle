using System;
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

    [Tooltip("The radius in which the light trigger can communicate with subscribers")]
    [SerializeField] private float _radius = 10f;
    private LightSensor _lightSensor;

    private void Awake()
    {
        _lightSensor = GetComponent<LightSensor>();
    }

    private void OnEnable()
    {
    }

    public void AddListener(UnityAction action)
    {
        UnityEvent e = new();
        e.AddListener(action);
    }

    private void OnDisable()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
