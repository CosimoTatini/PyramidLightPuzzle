using UnityEngine;

public class MummyObstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<Player>(out Player player))
        {
            player.SetDeath(DeathType.Mummy);
        }
        
    }
}
