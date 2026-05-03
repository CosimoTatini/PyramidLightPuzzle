using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<IVelocityProvider> _velocityGetters = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            _velocityGetters.Add(velocityProvider);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IVelocityProvider velocityProvider))
        {
            _velocityGetters.Remove(velocityProvider);
        }
    }

    public Vector2 Velocity
    {
        get
        {
            Vector2 vector2 = Vector2.zero;
            for (int i = _velocityGetters.Count - 1; i >= 0; i--)
            {
                if (_velocityGetters[i] == null)
                {
                    _velocityGetters.RemoveAt(i);
                    continue;
                }

                Vector2 velocity = _velocityGetters[i].Velocity;
                vector2 += velocity;
            }
            return vector2;
        }
    }
}
