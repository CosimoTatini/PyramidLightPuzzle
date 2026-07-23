using System.Linq;
using UnityEngine;

public class ThreeWayRGBSplitterInteraction : PriorityInteractable
{
    [SerializeField] private ThreeWayRGBSplit _threeWayRGBSplit;
    public override void Interact()
    {
        _threeWayRGBSplit.Rotate();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPriorityInteractableHost host))
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
