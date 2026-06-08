using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Player behaviour : animations, checkpoint reference , obstacle interactions, state machine inatialization.
/// </summary>
public class Player : MonoBehaviour, ISubject
{
    [Header("Animation Settings")]
    public AnimSettings IdleSettings;
    public AnimSettings WalkSettings;
    public AnimSettings PlaceSettings;
    public AnimSettings GrabSettings;
    public AnimSettings DeathSettings;
    public AnimSettings ThrowSettings;


    [Header("CheckpointSystem")]
    public List<Transform> CheckPoints = new List<Transform>();
    private Transform _currentCheckpoint;
    private bool _isRespawning;
    private List<IObserver> _observers = new List<IObserver>();
    public GenericStateMachine<ECharacterStates> StateMachine;
    [HideInInspector] public Animator Animator;
    private SpriteRenderer _renderer;

    private PlayerController _playerController;

    private IState _currentState;
    [SerializeField] private ECharacterStates _currentStateEnum;
    private DeathCharacterState _deathState;
    public GameObject DetectedObject { get; set; }
    [SerializeField] private Tilemap _placeableTilemap;
    [SerializeField] private GameObject _torchPrefab;
    private float _cellOffset = 0.2f;

    public Tilemap PlaceableTilemap => _placeableTilemap;
    public float CellOffset => _cellOffset;

    public GameObject TorchPrefab => _torchPrefab;

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
        StateMachine.RegisterState(ECharacterStates.Idle, new IdleCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Walk, new WalkCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Place, new PlaceCharacterState(this, _playerController, _placeableTilemap, _torchPrefab, Animator));
        StateMachine.RegisterState(ECharacterStates.Grab, new GrabCharacterState(this, _playerController, _torchPrefab, _placeableTilemap, Animator));
        StateMachine.RegisterState(ECharacterStates.Death, new DeathCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Throw, new ThrowCharacterState(this,_playerController,Animator));
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
    }

    private void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
    }

    private void OnEnable()

    {
        if(InventoryManager.Instance != null) 
        InventoryManager.Instance.OnSelectionChange += EquipEmitter;
    }

    private void OnDisable()
    {
        if(InventoryManager.Instance!= null)
        InventoryManager.Instance.OnSelectionChange -= EquipEmitter;
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

    public void SetDeath()
    {
        if (!_isRespawning)
        {
            _isRespawning = true;
            SetState(ECharacterStates.Death);
        }
    }

    public void Respawn()
    {
        if (_currentCheckpoint != null)
        {
            transform.position = _currentCheckpoint.transform.position;
            Debug.Log("Respawn done");
            Notify();
            _isRespawning = false;
        }

        else
        {
            Debug.LogWarning("No checkpoint saved found");
        }
    }



    public void EquipEmitter(GameObject newTorch)
    {
        _torchPrefab = newTorch;
    }

    public void HandleInteract()
    {
        if (StateMachine.CurrentState is DeathCharacterState || _isRespawning) return;

       
        if (InventoryManager.Instance.SelectedType == TorchType.Magical)
        {
            if (PlacementManager.Instance.FindMagicalTorch().HasValue)
            {
                SetState(ECharacterStates.Grab);
                return;
            }
        }
        Vector3Int currentCellPos = _placeableTilemap.WorldToCell(transform.position);

        if (!PlacementManager.Instance.IsCellAvailable(_placeableTilemap, currentCellPos))
        {
           
            if (InventoryManager.Instance.SelectedType == TorchType.Normal)
            {
                SetState(ECharacterStates.Grab);
                return;
            }
        }
        Vector3 targetWorldPos = transform.position + (Vector3)_playerController.LastLookDirection * _cellOffset;
        Vector3Int forwardCellPos = _placeableTilemap.WorldToCell(targetWorldPos);

        if (!PlacementManager.Instance.IsCellAvailable(_placeableTilemap, forwardCellPos))
        {
            Vector3 cellCenter = _placeableTilemap.GetCellCenterWorld(forwardCellPos);

            if (Vector2.Distance(transform.position, cellCenter) <= 0.6f)
            {
                SetState(ECharacterStates.Grab);
                return;
            }
        }

        SetState(ECharacterStates.Place);
    }

    public void FinishPlacing()
    {
        SetState(ECharacterStates.Idle);
    }

    public void HandleSwitch()
    {
        InventoryManager.Instance.SwitchSelection();
    }
}
