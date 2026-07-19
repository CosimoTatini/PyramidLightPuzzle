using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour, ILightTriggerReceiver, IVelocityProvider
{
    [SerializeField] private List<Transform> _wayPoints;
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private bool _reverseBehaviour = false;

    private float _timeElapsed;
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
        _timeElapsed = 0;
        transform.position = _wayPoints[_currentWaypoint].position;
#if UNITY_EDITOR
        _previousUseRadius = _useRadius;
#endif
    }

    private void FixedUpdate()
    {
        // avoid useless calculations, also protects from DIV by 0 error when calculating duration
        if (!_isMoving || _moveSpeed <= 0f)
        {
            Velocity = Vector2.zero;
            return;
        }

        Vector2 targetPos = _wayPoints[NextWayPoint].position;
        Vector2 startPos = _wayPoints[_currentWaypoint].position;
        
        float totalDistance = Vector2.Distance(startPos, targetPos);
        float duration = totalDistance/_moveSpeed;

        _timeElapsed += Time.fixedDeltaTime;

        // if duration is exactly 0 (meaning totalDistance is 0) we directly set t to 1, so we go to next waypoint
        float t = duration > 0 ? _timeElapsed/duration : 1f;

        Vector2 nextPos = Vector2.Lerp(startPos, targetPos, Mathf.Clamp01(t));

        // NOTE: this calculates the current velocity, even if as this line of code the velocity isn't changed yet, this makes
        // the passengers have the correct velocity, otherwise we would get an outdated one which leads to incorrect movement.
        // This also requires the platform to be ran before the passengers in the execution order, otherwise we would get the velocity of the previous frame which leads to a stuttering movement.
        Velocity = (nextPos - _rb.position) / Time.fixedDeltaTime;

        _rb.MovePosition(nextPos);
        //TODO: in case of a big stutter, we could've covered more than one waypoint, and we should account for that so doesn't matter how much time
        // passed, the platform is where it should, tho the current solution is very reasonable
        // we've reached the nextWayPoint
        if(t >= 1f)
        {
            _currentWaypoint = NextWayPoint;
            // subtract the duration, if this trip took extra time this will the balance next one (it will have slightly less time to reach the next waypoint), this removes tiny desyncs that can happen over time
            _timeElapsed -= duration;
            // Debug.Log($"{duration} {_timeElapsed}");
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

    public Vector2 Velocity
    {
        get; set;
    }

    //CompositeCollider2D fs;
    //PolygonCollider2D player;
    //player.points; => Convert to world space
    //    // check each point;
    //fs.OverlapPoint();

#if UNITY_EDITOR

    private bool _previousUseRadius;
    private void OnValidate()
    {
        if (Application.isPlaying && Time.time > 1f)
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
        _isMoving = _reverseBehaviour ? true : false;
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
        _isMoving = _reverseBehaviour ? false : true;
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
