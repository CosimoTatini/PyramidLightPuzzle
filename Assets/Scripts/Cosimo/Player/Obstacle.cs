using UnityEngine;

public class Obstacle : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent( out Player player ))
        {
           Debug.Log(collision.gameObject.name);
        }

    }
}
