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
    private Rigidbody2D _rb;
    private bool _isMoving = true;
    private bool _isReturning = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentWaypoint = 0;
        transform.position = _wayPoints[_currentWaypoint].position;
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

    private void OnLightActivated(LightTrigger lightTrigger)
    {
        _isMoving = false;
    }

    //TODO: doesn't work properly since the radius check compares this transform's center with its transform center, the problem is the add is called
    // when we trigger, but in that moment our center is not inside the radius, so we don't actually subscribe
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger))
        {
            trigger.AddOnLightActivatedListener(this, OnLightActivated);
            Debug.Log("subscribed to trigger: " + trigger.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out LightTrigger trigger))
        {
            // Cleanup
            _isMoving = true;

            trigger.RemoveOnLightActivatedListener(this, OnLightActivated);
            Debug.Log("unsubscribed to trigger: " + trigger.name);
        }
    }

    public void LightActivated(LightTrigger lightTrigger)
    {
        throw new System.NotImplementedException();
    }

    public void LightChanged(LightTrigger lightTrigger)
    {
        throw new System.NotImplementedException();
    }

    public void LightDeactivated(LightTrigger lightTrigger)
    {
        throw new System.NotImplementedException();
    }
}
