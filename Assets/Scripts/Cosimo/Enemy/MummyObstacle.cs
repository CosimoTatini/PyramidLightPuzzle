using UnityEngine;

public class MummyObstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Player player))
        {
            player.RespawnToFirst();
        }
    }
}
