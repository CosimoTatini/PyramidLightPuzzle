using System.Linq;
using UnityEngine;

public class ThreeWayRGBSplitterInteraction : PriorityInteraction
{
    [SerializeField] private ThreeWayRGBSplit _threeWayRGBSplit;

    private IPriorityInteractableHost _host;
    public override void Interact()
    {
        if (_threeWayRGBSplit.IsRotating)
        {
            return;
        }
        OnInteract.Invoke();
        _threeWayRGBSplit.Rotate();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            // _host = host;
            // _threeWayRGBSplit.OnRotationCompleted -= AddHostWhenRotationCompleted;
            // _threeWayRGBSplit.OnRotationCompleted += AddHostWhenRotationCompleted;
            // _threeWayRGBSplit.OnRotationStarted -= RemoveHostWhenRotationStarted;
            // _threeWayRGBSplit.OnRotationStarted += RemoveHostWhenRotationStarted;

            // if (!_threeWayRGBSplit.IsRotating)
            // {
                host.AddInteractable(this);
            // }
        }
    }

    private void AddHostWhenRotationCompleted()
    {
        if (_host != null)
        {
            _host.AddInteractable(this);
        }
    }

    private void RemoveHostWhenRotationStarted()
    {
        if (_host != null)
        {
            _host.RemoveInteractable(this);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
        {
            host.RemoveInteractable(this);
            // _host = null;
            // _threeWayRGBSplit.OnRotationCompleted -= AddHostWhenRotationCompleted;
            // _threeWayRGBSplit.OnRotationStarted -= RemoveHostWhenRotationStarted;
        }
    }
}
