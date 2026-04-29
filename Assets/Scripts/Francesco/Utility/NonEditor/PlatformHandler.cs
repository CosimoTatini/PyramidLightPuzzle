using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<PlatformVelocityGetter> _velocityGetters = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlatformVelocityGetter velocityGetter))
        {
            _velocityGetters.Add(velocityGetter);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlatformVelocityGetter velocityGetter))
        {
            _velocityGetters.Remove(velocityGetter);
        }
    }

    public Vector2? Velocity
    {
        get
        {
            Vector2 vector2 = Vector2.zero;
            for (int i = _velocityGetters.Count - 1; i >= 0; i--)
            {
                if (!_velocityGetters[i])
                {
                    _velocityGetters.RemoveAt(i);
                    continue;
                }

                Vector2? velocity = _velocityGetters[i].GetVelocity();
                vector2 += velocity ?? Vector2.zero;
            }
            return vector2;
        }
    }
}
