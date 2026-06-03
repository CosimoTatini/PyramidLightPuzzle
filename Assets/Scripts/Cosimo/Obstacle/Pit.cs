using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// If the player fully enter here, he dies.
/// </summary>
public class Pit : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float delayBeforeRespawn = 0.1f;
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


        _playerCollider = collision.GetComponent<Collider2D>();

        Debug.Log("contained:" + IsFullyContained(_pitCollider, collision) + " is touching ground:" + collision.IsTouchingLayers(_groundLayer));
        if (IsFullyContained(_pitCollider, collision))
        {
            
            if (collision.TryGetComponent(out Player player) && !collision.IsTouchingLayers(_groundLayer))
            {
                StartCoroutine(RespawnCoroutine(player));
            }
        }
    }

    private bool IsFullyContained(CompositeCollider2D container, Collider2D target)
    {
        Bounds b = target.bounds;

        Vector2[] pointsToTest = new Vector2[]
        {
        new Vector2(b.min.x, b.min.y),
        new Vector2(b.max.x, b.min.y),
        new Vector2(b.min.x, b.max.y),
        new Vector2(b.max.x, b.max.y),

        };

        foreach (Vector2 point in pointsToTest)
        {
            if (!container.OverlapPoint(point))
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator RespawnCoroutine(Player player)
    {
        _isRespawning = true;
        yield return new WaitForSeconds(delayBeforeRespawn);
        if (IsFullyContained(_pitCollider, _playerCollider) && !_playerCollider.IsTouchingLayers(_groundLayer))
        {
            player.SetDeath();
        }
        _isRespawning = false;
    }
}
