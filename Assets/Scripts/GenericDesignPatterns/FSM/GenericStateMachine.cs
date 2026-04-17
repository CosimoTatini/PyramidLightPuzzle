using System;
using System.Collections.Generic;
/// <summary>
/// Finite state machine
/// </summary>
/// To use it modularly, you can implement your own interface that extends IState
/// then identify which interface you are using in your script and call the specific methods you need,
/// for instance with IStateCollision2D you can call the collision methods
/// after checking if the current state implements that interface,
/// this way you can have a state machine that can handle multiple IState types, avoiding implementing extra methods in states that don't need them.
public class GenericStateMachine<T> where T : Enum
{
    private readonly Dictionary<T, IState> _allStates = new();

    private IState _currentState;
    private T _previousStateType;
    private T _currentStateType;

    public IState CurrentState => _currentState;
    public T PreviousStateType => _previousStateType; 
    public T CurrentStateType => _currentStateType;

    public void RegisterState(T stateType, IState state)
    {
        if (_allStates.ContainsKey(stateType))
        {
            throw new InvalidOperationException($"Esiste gi� uno stato {stateType}");
        }

        _allStates.Add(stateType, state);
    }

    public void SetState(T stateType)
    {
        if (!_allStates.ContainsKey(stateType))
        {
            throw new InvalidOperationException($"Non esiste alcuno stato {stateType}");
        }

        _previousStateType = _currentStateType; // for debug

        _currentState?.OnEnd(); // calling the exit method if currentState exists (first run)

        _currentStateType = stateType; // for debug

        _currentState = _allStates[stateType]; // selecting the state passed as parameter

        _currentState.OnStart(); // entering the new state
    }

    public void OnUpdate() => _currentState?.OnUpdate();
    public void OnFixedUpdate() => _currentState?.OnFixedUpdate();
}
