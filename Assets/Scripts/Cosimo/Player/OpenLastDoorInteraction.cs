using UnityEngine;

public class OpenLastDoorInteraction : PlacemenetRestricterInteraction
{
    [SerializeField] private LastDoor _lastDoor;

    public override void Interact()
    {
        if (_player == null) return;
        if (_placementRestricer == null) return;

        if (_lastDoor.AreAllFlamesActive())
        {
            _lastDoor.OpenDoor();
            OnInteract.Invoke();
            Destroy(this);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_lastDoor.AreAllFlamesActive() && !_lastDoor.IsDoorOpen && collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.AddInteractable(this);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.RemoveInteractable(this);
        }
    }
}