using UnityEngine;

public abstract class PlacemenetRestricterInteraction : PlayerPriorityInteractable
{
    [SerializeField] protected PlacementRestricter _placementRestricer;
    public PlacementRestricter PlacementRestricter => _placementRestricer;

    public override void Interact()
    {
        
    }
}
