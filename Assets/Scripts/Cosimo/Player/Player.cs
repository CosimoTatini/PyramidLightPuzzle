using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Player behaviour : animations, checkpoint reference , obstacle interactions, state machine inatialization.
/// </summary>
public class Player : MonoBehaviour, ISubject, IPriorityInteractableHost
{
    [Header("Animation Settings")]
    public AnimSettings IdleSettings;
    public AnimSettings WalkSettings;
    public AnimSettings PlaceSettings;
    public AnimSettings GrabSettings;
    public AnimSettings DeathSettings;
    public AnimSettings ThrowSettings;

    [Header("CheckpointSystem")]
    public Checkpoint CurrentCheckPoint;
    private bool _isRespawning;
    private List<IObserver> _observers = new List<IObserver>();
    public GenericStateMachine<ECharacterStates> StateMachine;
    [HideInInspector] public Animator Animator;
    private SpriteRenderer _renderer;

    private PlayerController _playerController;
    public PlayerController PlayerController => _playerController;

    private IState _currentState;
    [SerializeField] private ECharacterStates _currentStateEnum;
    private DeathCharacterState _deathState;
    public GameObject DetectedObject { get; set; }
    [SerializeField] private Tilemap _placeableTilemap;
    [SerializeField] private GameObject _torchPrefab;
    private float _cellOffset = 0.2f;
    [SerializeField] private Transform _feetTransform;

    [Header("Interaction")]
    private InteractableContextRegistry _interactableContextRegistry = new();
    private PriorityInteractableSet _interactPrioritySet;
    private PriorityInteractableSet _throwPrioritySet;
    private Dictionary<int, List<IPriorityInteractable>> _interactablesPriorityDict = new();
    [SerializeField] private RecallMagicalTorchInteraction _recallMagicalTorchInteraction;
    private bool _isTriggeringWithMagicalTorch = false;
    [SerializeField] private PlaceMagicalTorchInteraction _placeMagicalTorchInteraction;
    [SerializeField] private PlaceNormalTorchInteraction _placeNormalTorchInteraction;
    private IPriorityInteractable _currentInteractInteractable = null;
    private IPriorityInteractable _currentThrowInteractable = null;
    private int? _currentInteractableListKey;

    public Tilemap PlaceableTilemap => _placeableTilemap;
    public float CellOffset => _cellOffset;

    public GameObject TorchPrefab => _torchPrefab;

    public Transform FeetTransform => _feetTransform;

    public InteractableContextRegistry InteractableContextRegistry { get; private set; } = new();

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
        // Debug.Log($"[Subject] Notifica inviata a {_observers.Count} osservatori.");
        foreach (var item in _observers)
        {
            item.ObserverUpdate(this);
        }
    }
    private void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        _playerController = GetComponent<PlayerController>();
        _renderer = GetComponentInChildren<SpriteRenderer>();

        // _grabMagicalTorchInteraction = gameObject.AddComponent<GrabMagicalTorchInteraction>();
        // _grabNormalTorchInteraction = gameObject.AddComponent<GrabNormalTorchInteraction>();
        // _placeMagicalTorchInteraction = gameObject.AddComponent<PlaceMagicalTorchInteraction>();
        // _placeNormalTorchInteraction = gameObject.AddComponent<PlaceNormalTorchInteraction>();

        StateMachine = new GenericStateMachine<ECharacterStates>();
        StateMachine.RegisterState(ECharacterStates.Idle, new IdleCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Walk, new WalkCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Place, new PlaceCharacterState(this, _playerController, _placeableTilemap, _torchPrefab, Animator));
        StateMachine.RegisterState(ECharacterStates.Grab, new GrabCharacterState(this, _playerController, _torchPrefab, _placeableTilemap, Animator));
        StateMachine.RegisterState(ECharacterStates.Death, new DeathCharacterState(this, _playerController, Animator));
        StateMachine.RegisterState(ECharacterStates.Throw, new ThrowCharacterState(this, _playerController, Animator));
        SetState(ECharacterStates.Idle);
        _currentState = StateMachine.CurrentState;

        if (CurrentCheckPoint != null)
        {
            transform.position = CurrentCheckPoint.transform.position;
        }

        _interactPrioritySet = InteractableContextRegistry.GetOrCreatePriorityInteractableSet(_playerController.InputActions.Player.Interact.id.ToString());
        _throwPrioritySet = InteractableContextRegistry.GetOrCreatePriorityInteractableSet(_playerController.InputActions.Player.Throw.id.ToString());
        HandleTorchChange();
    }

    public void SetState(ECharacterStates state)
    {
        StateMachine.SetState(state);
        _currentState = StateMachine.CurrentState;
        _currentStateEnum = state;
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
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSelectionChange += EquipEmitter;
            InventoryManager.Instance.OnTorchChanged += HandleTorchChange;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSelectionChange -= EquipEmitter;
            InventoryManager.Instance.OnTorchChanged -= HandleTorchChange;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision.TryGetComponent(out Checkpoint checkpoint))
        // {
        //     CurrentCheckPoint = checkpoint.transform;

        //     if (!CheckPoints.Contains(CurrentCheckPoint))
        //     {
        //         CheckPoints.Add(CurrentCheckPoint);
        //     }
        // }

        if (_currentState is IStateCollision2D collisionState)
        {
            collisionState.OnTriggerEnter2D(collision);
        }

        if (collision.TryGetComponent(out RecallMagicalTorchInteraction interaction) && interaction.ItemPlacement != null && interaction.ItemPlacement.Collider2D != null && collision == interaction.ItemPlacement.Collider2D)
        {
            _isTriggeringWithMagicalTorch = true;
            HandleTorchChange();
        }

        // var interactables = collision.GetComponents<IPriorityInteractable>();

        // for (int i = 0; i < interactables.Length; i++)
        // {
        //     IPriorityInteractable interactable = interactables[i];
        //     if (interactable is ItemInteraction grabInteraction && grabInteraction.ItemPlacement != null && grabInteraction.ItemPlacement.Collider2D != null)
        //     {
        //         if (grabInteraction.ItemPlacement.Collider2D == collision)
        //         {
        //             // if it's magical torch don't add the one in the torch but the player's one
        //             if (grabInteraction is RecallMagicalTorchInteraction)
        //             {
        //                 _isTriggeringWithMagicalTorch = true;
        //                 AddInteractable(_recallMagicalTorchInteraction);
        //             }
        //             else
        //             {
        //                 AddInteractable(interactable);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         AddInteractable(interactable);
        //     }
        // }
        // if (collision.TryGetComponent(out IPriorityInteractable interactable))
        // {
        //     if (interactable is ItemInteraction grabInteraction && grabInteraction.ItemPlacement != null && grabInteraction.ItemPlacement.Collider2D != null)
        //     {
        //         if (grabInteraction.ItemPlacement.Collider2D == collision)
        //         {
        //             // if it's magical torch don't add the one in the torch but the player's one
        //             if (grabInteraction is RecallMagicalTorchInteraction)
        //             {
        //                 _isTriggeringWithMagicalTorch = true;
        //                 AddInteractable(_recallMagicalTorchInteraction);
        //             }
        //             else
        //             {
        //                 AddInteractable(interactable);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         AddInteractable(interactable);
        //     }
        // }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_currentState is IStateCollision2D collisionState)
        {
            collisionState.OnTriggerStay2D(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_currentState is IStateCollision2D collisionState)
        {
            collisionState.OnTriggerExit2D(collision);
        }

        if (collision.TryGetComponent(out RecallMagicalTorchInteraction interaction) && interaction.ItemPlacement != null && interaction.ItemPlacement.Collider2D != null && collision == interaction.ItemPlacement.Collider2D)
        {
            _isTriggeringWithMagicalTorch = false;
            HandleTorchChange();
        }
        // var interactables = collision.GetComponents<IPriorityInteractable>();

        // for (int i = 0; i < interactables.Length; i++)
        // {
        //     IPriorityInteractable interactable = interactables[i];
        //     if (interactable is ItemInteraction itemInteraction && itemInteraction.ItemPlacement != null && itemInteraction.ItemPlacement.Collider2D != null)
        //     {
        //         // if it's magical torch don't remove the one in the torch but the player's one
        //         if (itemInteraction is RecallMagicalTorchInteraction)
        //         {
        //             _isTriggeringWithMagicalTorch = false;
        //             HandleTorchChange();
        //         }
        //         else
        //         {
        //             RemoveInteractable(interactable);
        //         }
        //     }
        //     else
        //     {
        //         RemoveInteractable(interactable);
        //     }
        // }

        // if (collision.TryGetComponent(out IPriorityInteractable interactable))
        // {
        //     if (interactable is ItemInteraction itemInteraction && itemInteraction.ItemPlacement != null && itemInteraction.ItemPlacement.Collider2D != null)
        //     {
        //         // if it's magical torch don't remove the one in the torch but the player's one
        //         if (itemInteraction is RecallMagicalTorchInteraction)
        //         {
        //             _isTriggeringWithMagicalTorch = false;
        //             HandleTorchChange();
        //         }
        //         else
        //         {
        //             RemoveInteractable(interactable);
        //         }
        //     }
        //     else
        //     {
        //         RemoveInteractable(interactable);
        //     }
        // }
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
        if (CurrentCheckPoint != null)
        {
            transform.position = CurrentCheckPoint.transform.position;
            // Debug.Log("Respawn done");
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
        if (StateMachine.CurrentStateType != ECharacterStates.Walk && StateMachine.CurrentStateType != ECharacterStates.Idle || _isRespawning) return;

        if (_interactPrioritySet.InteractablesPriorityDict.Count == 0 || _currentInteractInteractable == null) return;

        if (_currentInteractInteractable is PlayerPriorityInteractable playerPriorityInteractable)
        {
            playerPriorityInteractable.SetPlayer(this);
        }
        _currentInteractInteractable.Interact();
        //if (InventoryManager.Instance.SelectedType == TorchType.Magical)
        //{
        //    if (PlacementManager.Instance.FindMagicalTorch().HasValue)
        //    {
        //        SetState(ECharacterStates.Grab);
        //        return;
        //    }
        //}
        //Vector3Int currentCellPos = _placeableTilemap.WorldToCell(transform.position);

        //if (!PlacementManager.Instance.IsCellAvailable(_placeableTilemap, currentCellPos))
        //{

        //    if (InventoryManager.Instance.SelectedType == TorchType.Normal)
        //    {
        //        SetState(ECharacterStates.Grab);
        //        return;
        //    }
        //}
        //Vector3 targetWorldPos = transform.position + (Vector3)_playerController.LastLookDirection * _cellOffset;
        //Vector3Int forwardCellPos = _placeableTilemap.WorldToCell(targetWorldPos);

        //if (!PlacementManager.Instance.IsCellAvailable(_placeableTilemap, forwardCellPos))
        //{
        //    Vector3 cellCenter = _placeableTilemap.GetCellCenterWorld(forwardCellPos);

        //    if (Vector2.Distance(transform.position, cellCenter) <= 0.6f)
        //    {
        //        SetState(ECharacterStates.Grab);
        //        return;
        //    }
        //}

        // SetState(ECharacterStates.Place);
    }

    public void HandleThrow()
    {
        if (StateMachine.CurrentStateType != ECharacterStates.Walk && StateMachine.CurrentStateType != ECharacterStates.Idle || _isRespawning) return;

        if (_throwPrioritySet.InteractablesPriorityDict.Count == 0 || _currentThrowInteractable == null) return;

        if (_currentThrowInteractable is PlayerPriorityInteractable playerPriorityInteractable)
        {
            playerPriorityInteractable.SetPlayer(this);
        }
        _currentThrowInteractable.Interact();
    }

    public void FinishPlacing()
    {
        SetState(ECharacterStates.Idle);
    }

    public void HandleSwitch()
    {
        InventoryManager.Instance.SwitchSelection();
    }

    public void CalculateCurrentInteractables()
    {
        if (_interactPrioritySet.CurrentInteractableListKey.HasValue)
        {
            var dict = _interactPrioritySet.InteractablesPriorityDict;
            List<IPriorityInteractable> priorityInteractables = dict[_interactPrioritySet.CurrentInteractableListKey.Value];
            if (priorityInteractables.Count == 0) return;

            IPriorityInteractable closerInteractable = priorityInteractables[0];
            float shortestDistance;
            if (closerInteractable is MonoBehaviour monoBehaviour)
            {
                shortestDistance = Vector2.Distance(monoBehaviour.transform.position, _feetTransform.position);
            }
            else
            {
                shortestDistance = float.MaxValue;
            }

            for (int i = 1; i < priorityInteractables.Count; i++)
            {
                float distance;
                IPriorityInteractable currentInteractable = priorityInteractables[i];
                if (currentInteractable is MonoBehaviour monoBehaviour2)
                {
                    distance = Vector2.Distance(monoBehaviour2.transform.position, _feetTransform.position);
                }
                else
                {
                    distance = float.MaxValue;
                }

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closerInteractable = currentInteractable;
                }
            }

            if (closerInteractable != null && closerInteractable != _currentInteractInteractable)
            {
                if (_currentInteractInteractable != null)
                {
                    InputConfigManager.UnregisterConfig(_currentInteractInteractable.InputConfigSO);
                }
                InputConfigManager.RegisterConfig(closerInteractable.InputConfigSO);
                _currentInteractInteractable = closerInteractable;
                // Debug.Log("UPDATE " + _currentInteractable);
            }
            else
            {
                // Debug.Log("UPDATE POLLO " + _currentInteractable);
            }
        }
        if (_throwPrioritySet.CurrentInteractableListKey.HasValue)
        {
            var dict = _throwPrioritySet.InteractablesPriorityDict;
            List<IPriorityInteractable> priorityInteractables = dict[_throwPrioritySet.CurrentInteractableListKey.Value];
            if (priorityInteractables.Count == 0) return;

            IPriorityInteractable closerInteractable = priorityInteractables[0];
            float shortestDistance;
            if (closerInteractable is MonoBehaviour monoBehaviour)
            {
                shortestDistance = Vector2.Distance(monoBehaviour.transform.position, _feetTransform.position);
            }
            else
            {
                shortestDistance = float.MaxValue;
            }

            for (int i = 1; i < priorityInteractables.Count; i++)
            {
                float distance;
                IPriorityInteractable currentInteractable = priorityInteractables[i];
                if (currentInteractable is MonoBehaviour monoBehaviour2)
                {
                    distance = Vector2.Distance(monoBehaviour2.transform.position, _feetTransform.position);
                }
                else
                {
                    distance = float.MaxValue;
                }

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closerInteractable = currentInteractable;
                }
            }

            if (closerInteractable != null && closerInteractable != _currentThrowInteractable)
            {
                if (_currentThrowInteractable != null)
                {
                    InputConfigManager.UnregisterConfig(_currentThrowInteractable.InputConfigSO);
                }
                InputConfigManager.RegisterConfig(closerInteractable.InputConfigSO);
                _currentThrowInteractable = closerInteractable;
                // Debug.Log("UPDATE " + _currentInteractable);
            }
            else
            {
                // Debug.Log("UPDATE POLLO " + _currentInteractable);
            }
        }
    }

    private void HandleTorchChange()
    {
        var invMan = InventoryManager.Instance;
        RemoveInteractable(_placeNormalTorchInteraction);
        RemoveInteractable(_recallMagicalTorchInteraction);
        RemoveInteractable(_placeMagicalTorchInteraction);
        if (invMan.SelectedType == TorchType.Magical || _isTriggeringWithMagicalTorch)
        {
            if (invMan.CurrentMagicTorchQuantity > 0)
            {
                AddInteractable(_placeMagicalTorchInteraction);
            }
            else
            {
                AddInteractable(_recallMagicalTorchInteraction);
            }
        }
        else if (invMan.SelectedType == TorchType.Normal)
        {
            if (invMan.CurrentTorchQuantity > 0)
            {
                AddInteractable(_placeNormalTorchInteraction);
            }
            else
            {
                if (invMan.CurrentMagicTorchQuantity < 1)
                {
                    AddInteractable(_recallMagicalTorchInteraction);
                }
            }
        }
    }

    public void AddInteractable(IPriorityInteractable priorityInteractable)
    {
        if (priorityInteractable == null) return;

        var entry = priorityInteractable.GetFirstEntry();
        if (entry == null) return;
        var priorityInteractableSet = InteractableContextRegistry.GetOrCreatePriorityInteractableSet(entry.Guid);

        if (priorityInteractableSet.AddInteractable(priorityInteractable))
        {
            // Interact interactable
            if (priorityInteractableSet == _interactPrioritySet)
            {

            }
        }
        else
        {

        }

        // bool createdNewList = false;
        // if (!_interactablesPriorityDict.ContainsKey(entry.Priority))
        // {
        //     _interactablesPriorityDict[entry.Priority] = new();
        //     createdNewList = true;
        // }

        // if (_interactablesPriorityDict[entry.Priority].Contains(interactable)) return;

        // if (interactable is PlayerPriorityInteractable playerPriorityInteractable)
        // {
        //     playerPriorityInteractable.SetPlayer(this);
        // }
        // _interactablesPriorityDict[entry.Priority].Add(interactable);
        // Debug.Log("Added" + interactable);
        // if (createdNewList)
        // {
        //     Debug.Log("Add Recalculated key");
        //     int? highestPriorityKey = _interactablesPriorityDict.Keys.ElementAt(0);
        //     foreach (var priority in _interactablesPriorityDict.Keys)
        //     {
        //         if (priority > highestPriorityKey)
        //         {
        //             highestPriorityKey = priority;
        //         }
        //     }
        //     _currentInteractableListKey = highestPriorityKey;
        // }
    }

    public void RemoveInteractable(IPriorityInteractable priorityInteractable)
    {
        if (priorityInteractable == null) return;

        var entry = priorityInteractable.GetFirstEntry();
        if (entry == null) return;

        var priorityInteractableSet = InteractableContextRegistry.GetOrCreatePriorityInteractableSet(entry.Guid);

        if (priorityInteractableSet.RemoveInteractable(priorityInteractable))
        {
            // Interact interactable
            if (priorityInteractableSet == _interactPrioritySet)
            {
                if (priorityInteractable == _currentInteractInteractable)
                {
                    InputConfigManager.UnregisterConfig(priorityInteractable.InputConfigSO);
                    _currentInteractInteractable = null;
                }
            }
            else if (priorityInteractableSet == _throwPrioritySet)
            {
                if (priorityInteractable == _currentThrowInteractable)
                {
                    InputConfigManager.UnregisterConfig(priorityInteractable.InputConfigSO);
                    _currentThrowInteractable = null;
                }
            }
        }
        else
        {

        }

        // if (!_interactablesPriorityDict.ContainsKey(entry.Priority)) return;

        // if (!_interactablesPriorityDict[entry.Priority].Contains(interactable)) return;

        // _interactablesPriorityDict[entry.Priority].Remove(item: interactable);
        // Debug.Log("REMOVED" + interactable);

        // if (_interactablesPriorityDict[entry.Priority].Count == 0)
        // {
        //     Debug.Log("Remove Recalculated key");
        //     _interactablesPriorityDict.Remove(entry.Priority);
        //     int? highestPriorityKey = _interactablesPriorityDict.Keys.Count > 0 ? _interactablesPriorityDict.Keys.ElementAt(0) : null;
        //     foreach (var priority in _interactablesPriorityDict.Keys)
        //     {
        //         if (priority > highestPriorityKey)
        //         {
        //             highestPriorityKey = priority;
        //         }
        //     }
        //     _currentInteractableListKey = highestPriorityKey;
        // }
    }

    public bool ContainsInteractable(IPriorityInteractable priorityInteractable)
    {
        if (priorityInteractable == null) return false;

        var entry = priorityInteractable.GetFirstEntry();
        if (entry == null) return false;

        var priorityInteractableSet = InteractableContextRegistry.TryGetPriorityInteractableSet(entry.Guid);
        if (priorityInteractableSet == null) return false;

        if (priorityInteractableSet.InteractablesPriorityDict.TryGetValue(entry.Priority, out var priorityInteractables))
        {
            return priorityInteractables.Contains(priorityInteractable);
        }

        return false;
    }
}
