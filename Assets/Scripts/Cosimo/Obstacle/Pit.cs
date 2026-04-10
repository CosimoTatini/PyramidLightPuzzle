using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private float delayBeforeRespawn = 1f;
    private bool _isRespawning = false;

    private CompositeCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<CompositeCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isRespawning || collision.GetComponent<Player>() == null) return;

        if (IsFullyContained(_collider, collision))
        {
            if (collision.TryGetComponent(out Player player))
            {
                StartCoroutine(RespawnCoroutine(player));
            }
        }
    }

    private bool IsFullyContained(CompositeCollider2D container, Collider2D target)
    {
        Bounds containerBounds = container.bounds;
        Bounds targetBounds = target.bounds;

        return targetBounds.min.x >= containerBounds.min.x &&
               targetBounds.max.x <= containerBounds.max.x &&
               targetBounds.min.y >= containerBounds.min.y &&
               targetBounds.max.y <= containerBounds.max.y;
    }

    private IEnumerator RespawnCoroutine(Player player)
    {
        _isRespawning = true;
        yield return new WaitForSeconds(delayBeforeRespawn);
        player.Respawn();
        _isRespawning = false;
    }
}
