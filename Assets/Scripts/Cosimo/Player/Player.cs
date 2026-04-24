
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public class Player : MonoBehaviour,ISubject
{
    private InventoryManager _inventoryManager;

    [Header("CheckpointSystem")]
    public List<Transform> CheckPoints = new List<Transform>();
    private Transform _currentCheckpoint;
    [SerializeField] private float _fallDuration = 0.5f;
    [SerializeField] private AnimationCurve _fallCurve;
    private bool _isRespawning;
    private List<IObserver> _observers= new List<IObserver>();
    public GenericStateMachine<ECharacterStates> StateMachine;
    [HideInInspector] public Animator Animator;
    private SpriteRenderer _renderer;

    private PlayerController _playerController;

    private IState _currentState;

    private DeathCharacterState _deathState;

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
        _inventoryManager = FindFirstObjectByType <InventoryManager>();
        Animator = GetComponentInChildren<Animator>();
        _playerController = GetComponent<PlayerController>();
        _renderer= GetComponentInChildren<SpriteRenderer>();


        StateMachine = new GenericStateMachine<ECharacterStates>();
        _deathState = new DeathCharacterState(this, _playerController);

        StateMachine.RegisterState(ECharacterStates.Idle, new IdleCharacterState(this, _playerController));
        StateMachine.RegisterState(ECharacterStates.Walk, new WalkCharacterState(this, _playerController));
        //StateMachine.RegisterState(ECharacterStates.Place, new PlaceCharacterState(this, _playerController));
        //StateMachine.RegisterState(ECharacterStates.Grab, new PlaceCharacterState(this, _playerController));
        StateMachine.RegisterState(ECharacterStates.Death, new DeathCharacterState(this, _playerController));
        //StateMachine.RegisterState(ECharacterStates.Throw, new ThrowCharacterState(this, _playerController));

        StateMachine.SetState(ECharacterStates.Idle);
        _currentState= StateMachine.CurrentState;
    }

    public void SetState(ECharacterStates state)
    {
        StateMachine.SetState(state);
        _currentState = StateMachine.CurrentState;
    }

    private void Start()
    {
        if(CheckPoints.Count > 0)
        {
            _currentCheckpoint= CheckPoints[0];
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
        if(collision.TryGetComponent<Checkpoint>(out Checkpoint checkpoint))
        {
            _currentCheckpoint = checkpoint.transform;

            if(!CheckPoints.Contains(_currentCheckpoint))
            {
                CheckPoints.Add(_currentCheckpoint);
                Debug.Log("Checkpoint reached:" + checkpoint.CheckpointID);
            }
            
        }

        if(_currentState is IStateCollision2D collisionState)
        {
            collisionState.OnTriggerEnter2D(collision);
        }

        if(!_isRespawning)
        {
            if(collision.TryGetComponent<MummyObstacle>(out var mummy))
            {
                _deathState.SetUpDeath(true);
                SetState(ECharacterStates.Death);
            }

            else if(collision.TryGetComponent<Obstacle>(out var obstacle))
            {
                _deathState.SetUpDeath(false);
                SetState(ECharacterStates.Death);
            }
        }

    }


    private IEnumerator FallAndRespawnCoroutine()
    {
        _isRespawning = true;

        float timer = 0f;
        Vector3 startScale = transform.localScale;
        while (timer < _fallDuration)
        {
            timer += Time.deltaTime;
            float t = timer / _fallDuration;

            float scale = Mathf.Lerp(0.5f,1f,_fallCurve.Evaluate(t));

            transform.localScale = new Vector3(scale,scale,scale);
            yield return null;

        }
        Respawn();
        transform.localScale = startScale;
        _isRespawning=false;
    }

    public void Respawn()
    {
        if(_currentCheckpoint!=null)
        {
            transform.position= _currentCheckpoint.transform.position;
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
        if(CheckPoints.Count>0 && !_isRespawning)
        {
            _currentCheckpoint= CheckPoints[0];
            StartCoroutine(FallAndRespawnCoroutine());
        }
    }
}
