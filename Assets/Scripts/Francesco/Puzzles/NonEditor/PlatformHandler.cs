using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent _onStop;
    [SerializeField] private UnityEvent _onMove;
    public event Action OnStopAction;
    public event Action OnMoveAction;

    private List<IVelocityProvider> _velocityProviders = new();
    private static float MAGNITUDE_THRESHOLD = 0.00001f;

    private void Awake()
    {
        if (TryGetComponent(out IVelocityProvider localProvider))
        {
            if (!_velocityProviders.Contains(localProvider))
            {
                _velocityProviders.Add(localProvider);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            Vector2 previousVelocity = Velocity;
            _velocityProviders.Add(velocityProvider);
            // platform started moving because of thie new velocity provider
            if (!IsMoving(previousVelocity) && IsMoving(Velocity))
            {
                _onMove.Invoke();
                OnMoveAction?.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            Vector2 previousVelocity = Velocity;
            _velocityProviders.Remove(velocityProvider);
            // platform stopped
            if (IsMoving(previousVelocity) && !IsMoving(Velocity))
            {
                _onStop.Invoke();
                OnStopAction?.Invoke();
            }
        }
    }

    private bool IsMoving(Vector2 vector2)
    {
        return vector2.sqrMagnitude >= MAGNITUDE_THRESHOLD;
    }

    public bool IsMoving()
    {
        return IsMoving(Velocity);
    }

    public Vector2 Velocity
    {
        get
        {
            Vector2 vector2 = Vector2.zero;
            for (int i = _velocityProviders.Count - 1; i >= 0; i--)
            {
                if (_velocityProviders[i] == null)
                {
                    _velocityProviders.RemoveAt(i);
                    continue;
                }

                Vector2 velocity = _velocityProviders[i].Velocity;
                vector2 += velocity;
            }
            return vector2;
        }
    }
}
