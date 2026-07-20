using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TeleporterDestination _destination;


    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<PlayerController>(out var player))
        {
            PerformTeleport(player);
        }
    }

    private void PerformTeleport(PlayerController player)
    {
        if (_destination == null) return;

        player.ResetMoveDirection();

        Rigidbody2D rb = player.Rb;

        Collider2D collider = player.Collider;

        if(collider != null)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.MovePosition(_destination.GetDestinationPosition());


        } 

        else
        {
            player.transform.position = _destination.GetDestinationPosition();
        }

        if(collider != null)
        {
            collider.enabled = true;
        }

    }
}
