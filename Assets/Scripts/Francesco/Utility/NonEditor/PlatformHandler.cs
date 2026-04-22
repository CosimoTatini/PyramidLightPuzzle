using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _platformBody;

    private Dictionary<Rigidbody2D, Vector2> _passengersVelocities;
    private Dictionary<Rigidbody2D, Coroutine> _passengersCoroutines;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // how do we recognize platform?
        // passengers need to have a reference to this script,
        // this means we can simply use a method to subscribe/unsubscribe from this, thus we already know which objects are going
        // to use the handler
        if (collision.attachedRigidbody)
        { 
            
        }
    }

    private void FixedUpdate()
    {
        
    }
}
