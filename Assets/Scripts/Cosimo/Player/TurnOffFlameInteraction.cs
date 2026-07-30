using System;
using UnityEngine;

public class TurnOffFlameInteraction : PlacemenetRestricterInteraction
{
    [SerializeField] private BigFlame _bigFlame;

    void OnEnable()
    {
        _bigFlame.OnFlameOff.RemoveListener(BigFlameTurnedOff);
        _bigFlame.OnFlameOff.AddListener(BigFlameTurnedOff);
    }

    void OnDisable()
    {
        _bigFlame.OnFlameOff.RemoveListener(BigFlameTurnedOff);
    }

    private void BigFlameTurnedOff()
    {
        if (_host != null)
        {
            _host.RemoveInteractable(this);
        }

        // Remove possibility for player to trigger the interaction again
        Destroy(this);
    }

    public override void Interact()
    {
        if (_player == null) return;
        if (_placementRestricer == null) return;

        OnInteract.Invoke();
        _bigFlame.TurnOff();
        _player.SetState(ECharacterStates.Grab);
    }

    private IPriorityInteractableHost _host;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.AddInteractable(this);
            _host = host;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.RemoveInteractable(this);
            _host = null;
        }
    }
}