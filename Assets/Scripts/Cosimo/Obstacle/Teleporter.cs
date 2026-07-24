using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TeleporterDestination _destination;

    private void Awake()
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            PerformTeleport(player);
        }
    }

    private void PerformTeleport(PlayerController player)
    {
        if (_destination == null) return;

       
        player.ResetMoveDirection();

       
        player.transform.position = _destination.GetDestinationPosition();
    }
}