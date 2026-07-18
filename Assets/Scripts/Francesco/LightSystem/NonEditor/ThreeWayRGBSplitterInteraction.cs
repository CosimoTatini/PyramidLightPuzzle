using System.Linq;
using UnityEngine;

public class ThreeWayRGBSplitterInteraction : PriorityInteractable
{
    [SerializeField] private ThreeWayRGBSplit _threeWayRGBSplit;
    public override void Interact()
    {
        _threeWayRGBSplit.Rotate();
    }
}
