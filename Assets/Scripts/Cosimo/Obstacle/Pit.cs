using System.Collections;
using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private float delayBeforeRespawn = 1f;
    private bool _isRespawning = false;



    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Player player))
        {
           if(GetComponent<Collider2D>().bounds.Contains(collision.bounds.min)
           && GetComponent<Collider2D>().bounds.Contains(collision.bounds.max))
           {

           }

            ColliderDistance2D dist = Physics2D.Distance(GetComponent<Collider2D>(), collision);
            dist.isOverlapped 


        }
       
    }


    private IEnumerator RespawnCoroutine(Player player)
    {
        yield return new WaitForSeconds(delayBeforeRespawn);
        player.Respawn();
        _isRespawning = false;
    }
}
