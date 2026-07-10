using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<IVelocityProvider> _velocityProviders = new();
    public event Action OnStop;
    public event Action OnMove;
    private static float MAGNITUDE_THRESHOLD = 0.00001f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            Vector2 previousVelocity = Velocity;
            _velocityProviders.Add(velocityProvider);
            // consider the platform moving
            if (previousVelocity.sqrMagnitude > MAGNITUDE_THRESHOLD && Velocity.sqrMagnitude > MAGNITUDE_THRESHOLD)
            {

            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            _velocityProviders.Remove(velocityProvider);
        }
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
