using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private float delayBeforeRespawn = 1f;
    private bool _isRespawning = false;

    private CompositeCollider2D _pitCollider;
    private Collider2D _playerCollider;

    private void Awake()
    {
        _pitCollider = GetComponent<CompositeCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isRespawning || collision.GetComponent<Player>() == null) return;

        
        _playerCollider= collision.GetComponent<Collider2D>();

        if (IsFullyContained(_pitCollider, collision))
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
        if(IsFullyContained(_pitCollider,_playerCollider))
        {
            player.Respawn();
        }
        _isRespawning = false;
    }
}
