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
        Bounds containerBounds = container.bounds;
        Bounds targetBounds = target.bounds;

        Debug.Log(container.bounds);
        Debug.Log(target.bounds);

        Debug.Log("Palla");

        Debug.Log($"{targetBounds.min.x >= containerBounds.min.x}" +
        $"{targetBounds.max.x <= containerBounds.max.x}" +
          $"{targetBounds.min.y >= containerBounds.min.y}" +
            $"{targetBounds.max.y <= containerBounds.max.y}");

        return targetBounds.min.x > containerBounds.min.x &&
           targetBounds.max.x < containerBounds.max.x &&
          targetBounds.min.y > containerBounds.min.y &&
          targetBounds.max.y > containerBounds.max.y;

    }

    private IEnumerator RespawnCoroutine(Player player)
    {
        _isRespawning = true;
        yield return new WaitForSeconds(delayBeforeRespawn);
        player.Respawn();
        _isRespawning = false;
    }
}
