using UnityEngine;

public class Obstacle : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            player.SetDeath(DeathType.Normal);
            Debug.Log(collision.gameObject.name);
        }

    }
}
