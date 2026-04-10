using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private float delayBeforeRespawn = 1f;
    private bool _isRespawning = false;

    private CompositeCollider2D _collider;

    private List<Collider2D> _colliders = new List<Collider2D>();

    private void Awake()
    {
        _collider = GetComponent<CompositeCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (_isRespawning || collision.GetComponent<Player>() == null) return;

        Debug.DrawLine(collision.bounds.min, collision.bounds.max, Color.red);
        if (IsFullyContained(_collider, collision))
        {
            Debug.Log("Boh");
            if (collision.TryGetComponent(out Player player))
            {
                Debug.Log("Bah");
                StartCoroutine(RespawnCoroutine(player));
            }
        }
    }



    private bool IsFullyContained(CompositeCollider2D container, Collider2D target)
    {
        //Bounds containerBounds = container.bounds;
        //Bounds targetBounds = target.bounds;

        //Debug.Log(container.bounds);
        //Debug.Log(target.bounds);

        //Debug.Log("Palla");

        //Debug.Log($"{targetBounds.min.x >= containerBounds.min.x}" +
        //$"{targetBounds.max.x <= containerBounds.max.x}" +
        //  $"{targetBounds.min.y >= containerBounds.min.y}" +
        //    $"{targetBounds.max.y <= containerBounds.max.y}");

        //return targetBounds.min.x > containerBounds.min.x &&
        //   targetBounds.max.x < containerBounds.max.x &&
        //  targetBounds.min.y > containerBounds.min.y &&
        //  targetBounds.max.y > containerBounds.max.y;

        ContactFilter2D filter = new ContactFilter2D();

        filter.useTriggers = true;

        int count = container.Overlap(filter, _colliders);

        for(int i = 0; i < count; i++)
        {
            if (_colliders[i] == target)
            {
                return CheckFullContainment(container, target);
            }
        }
        return false;

    }

    private bool CheckFullContainment(CompositeCollider2D container, Collider2D target)
    {
        Bounds b = target.bounds;

        Vector2[] points = new Vector2[]
        {
            new Vector2(b.min.x, b.min.y),
            new Vector2(b.max.x, b.min.y),
            new Vector2(b.min.x,b.max.y),
            new Vector2(b.max.x,b.max.y),
            b.center
        };

        foreach (Vector2 point in points)
        {
            Debug.Log($"{!container.OverlapPoint(point)}");
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
        player.Respawn();
        _isRespawning = false;
    }
}
