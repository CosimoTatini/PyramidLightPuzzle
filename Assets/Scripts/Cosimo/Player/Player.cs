
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Player : MonoBehaviour, ISubject
{


    [Header("CheckpointSystem")]
    public List<Transform> CheckPoints = new List<Transform>();
    private Transform _currentCheckpoint;
    [SerializeField] private float _fallDuration = 0.5f;
    [SerializeField] private AnimationCurve _fallCurve;
    private bool _isRespawning;
    private List<IObserver> _observers = new List<IObserver>();
    public GenericStateMachine<ECharacterStates> StateMachine;
    [HideInInspector] public Animator Animator;
    private SpriteRenderer _renderer;

    private PlayerController _playerController;

    private IState _currentState;
    [SerializeField] private ECharacterStates _currentStateEnum;
    private DeathCharacterState _deathState;
    [SerializeField] private DeathType _deathType;

    [SerializeField] private Tilemap _placeableTilemap;
    [SerializeField] private GameObject _torchPrefab;

    public DeathType Deathtype => _deathType;

    public void Attach(IObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            Debug.Log($"[Subject] {observer} si è registrato correttamente!");
            _observers.Add(observer);
        }
    }

    public void Detach(IObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify()
    {
        Debug.Log($"[Subject] Notifica inviata a {_observers.Count} osservatori.");
        foreach (var item in _observers)
        {
            item.ObserverUpdate(this);
        }
    }
    private void Awake()
    {

        Animator = GetComponentInChildren<Animator>();
        _playerController = GetComponent<PlayerController>();
        _renderer = GetComponentInChildren<SpriteRenderer>();


        StateMachine = new GenericStateMachine<ECharacterStates>();
        _deathState = new DeathCharacterState(this, _playerController);

        StateMachine.RegisterState(ECharacterStates.Idle, new IdleCharacterState(this, _playerController));
        StateMachine.RegisterState(ECharacterStates.Walk, new WalkCharacterState(this, _playerController));
        StateMachine.RegisterState(ECharacterStates.Place, new PlaceCharacterState(this, _playerController, _placeableTilemap, _torchPrefab));
        StateMachine.RegisterState(ECharacterStates.Grab, new GrabCharacterState(this, _playerController, _torchPrefab, _placeableTilemap));
        StateMachine.RegisterState(ECharacterStates.Death, new DeathCharacterState(this, _playerController));
        //StateMachine.RegisterState(ECharacterStates.Throw, new ThrowCharacterState(this, _playerController));

        StateMachine.SetState(ECharacterStates.Idle);
        _currentState = StateMachine.CurrentState;
    }

    public void SetState(ECharacterStates state)
    {
        StateMachine.SetState(state);
        _currentState = StateMachine.CurrentState;
        _currentStateEnum = state;
    }

    private void Start()
    {
        if (CheckPoints.Count > 0)
        {
            _currentCheckpoint = CheckPoints[0];
        }
        transform.position = CheckPoints[0].position;
    }

    private void Update()
    {
        _currentState?.OnUpdate();
        Debug.Log("Statemachine update attivo");
    }

    private void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
        Debug.Log(" Statemachine fixedupdate attivo");
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Checkpoint>(out Checkpoint checkpoint))
        {
            _currentCheckpoint = checkpoint.transform;

            if (!CheckPoints.Contains(_currentCheckpoint))
            {
                CheckPoints.Add(_currentCheckpoint);
                Debug.Log("Checkpoint reached:" + checkpoint.CheckpointID);
            }

        }

        if (_currentState is IStateCollision2D collisionState)
        {
            collisionState.OnTriggerEnter2D(collision);
        }

    }
    public void SetDeath(DeathType type)
    {
        _deathType = type;
        SetState(ECharacterStates.Death);

 
    }

    public void Respawn()
    {
        if (_currentCheckpoint != null)
        {
            transform.position = _currentCheckpoint.transform.position;
            Debug.Log("Respawn done");
            Notify();
        }

        else
        {
            Debug.LogWarning("No checkpoint saved found");
        }
    }

    public void RespawnToFirst()
    {
        if (CheckPoints.Count > 0 && !_isRespawning)
        {
            _currentCheckpoint = CheckPoints[0];
        }
    }

    public void EquipEmitter(GameObject newTorch)
    {
        _torchPrefab = newTorch;
    }

    public void HandleInteract()
    {
        if (StateMachine.CurrentState is DeathCharacterState || _isRespawning) return;

        Vector3 targetWorldPos = transform.position + (Vector3)_playerController.LastLookDirection * 0.8f;

        Debug.Log("Premuto E");
        SetState(ECharacterStates.Place);
    }

    public void FinishPlacing()
    {
        SetState(ECharacterStates.Idle);
    }

    public void HandleSwitch()
    {
        //_inventoryManager.ChangeSelection();
        //EquipEmitter(_inventoryManager.GetSelectedItem().Prefab);
    }
}
