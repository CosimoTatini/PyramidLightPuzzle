using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour, ILightTriggerReceiver
{
    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private float _moveSpeed = 5f;

    private int _currentWaypoint = 0;
    private int NextWayPoint
    {
        get
        {
            if (_wayPoints.Count == 0) return -1;
            if (_wayPoints.Count == 1) return 0;

            if (_isReturning)
            {
                if (_currentWaypoint - 1 > -1)
                {
                    return _currentWaypoint - 1;
                }
                else
                {
                    _isReturning = false;
                    return 1;
                }
            }
            else
            {
                if (_currentWaypoint + 1 < _wayPoints.Count)
                {
                    return _currentWaypoint + 1;
                }
                else
                {
                    _isReturning = true;
                    return _wayPoints.Count - 2;
                }
            }

        }
    }

    public LightTrigger LightTrigger { get; private set; }

    private Rigidbody2D _rb;
    private bool _isMoving = true;
    private bool _isReturning = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentWaypoint = 0;
        transform.position = _wayPoints[_currentWaypoint].position;
#if UNITY_EDITOR
        _previousUseRadius = _useRadius;
#endif
    }

    private void FixedUpdate()
    {
        if (!_isMoving) return;

        Vector2 targetPos = _wayPoints[NextWayPoint].position;
        Vector2 reachPos = Vector2.MoveTowards(_rb.position, targetPos, _moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(reachPos);
        if (Vector2.Distance(targetPos, _rb.position) <= 0.1f)
        {
            _currentWaypoint = NextWayPoint;
        }
    }

    [SerializeField] private bool _useRadius;
    private bool _IsInsideRadius = false;

    public bool UseRadius
    {
        get
        {
            return _useRadius;
        }
        set
        {
#if UNITY_EDITOR
            bool previousValue = _previousUseRadius;
#else
            bool previousValue = _useRadius;
#endif
            _useRadius = value;
            // value changed
            if (previousValue != _useRadius)
            {
                // using radius
                if (_useRadius)
                {
                    // if outside we need to check if the light is active, if so call deactivated
                    if (!_IsInsideRadius && LightTrigger.IsActive)
                    {
                        LightDeactivated();
                    }
                }
                // not using radius
                else
                {
                    // if outside we need to check if light is active, if so activate it
                    if (!_IsInsideRadius && LightTrigger.IsActive)
                    {
                        LightActivated();
                    }
                }
            }
        }
    }

#if UNITY_EDITOR

    private bool _previousUseRadius;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Debug.Log(_previousUseRadius + " " + _useRadius);
            if (_previousUseRadius != _useRadius)
            {
                UseRadius = _useRadius;
                _previousUseRadius = _useRadius;
            }
        }
    }

#endif

    //TODO: doesn't work properly since the radius check compares this transform's center with its transform center, the problem is the add is called
    // when we trigger, but in that moment our center is not inside the radius, so we don't actually subscribe
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            _IsInsideRadius = true;
            if (!_useRadius) return;

            if (LightTrigger.IsActive)
            {
                LightActivated();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger) && trigger == LightTrigger)
        {
            if (_useRadius) LightDeactivated();
            _IsInsideRadius = false;
        }
    }

    public void LightActivated()
    {
        if (!_useRadius) goto LightActivatedAction;

        if (!_IsInsideRadius)
        {
            return;
        }

    LightActivatedAction:
        _isMoving = false;
    }

    public void LightChanged()
    {
        // we don't need it in this case
    }

    public void LightDeactivated()
    {
        if (!_useRadius) goto LightDeactivatedAction;

        if (!_IsInsideRadius)
        {
            return;
        }

    LightDeactivatedAction:
        _isMoving = true;
    }

    public void SetLightTrigger(LightTrigger lightTrigger)
    {
        LightTrigger = lightTrigger;
        if (!_useRadius)
        {
            if (LightTrigger.IsActive)
            {
                LightActivated();
            }
            else
            {
                LightDeactivated();
            }
        }
        // Add Physics overlap if we want to check if we are already inside the new trigger (this doesn't happen for now since we set the trigger on awake and
        // it's not planned to change runtime)
    }
}
