using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField] private LayerMask _playerMask;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject)
        {

        }
    }


}
