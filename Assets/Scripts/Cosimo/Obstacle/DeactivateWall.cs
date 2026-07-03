using System;
using UnityEngine;

public class DeactivateWall : MonoBehaviour,ILightTriggerReceiver
{
    public LightTrigger LightTrigger {  get; private set; }

    private void OnEnable()
    {
        PlacementManager.OnEternalTorchRemoved += HandleTorchRemoved;
    }

    private void OnDisable()
    {
        PlacementManager.OnEternalTorchRemoved -= HandleTorchRemoved;
    }

    public void HandleTorchRemoved()
    {
        Destroy(gameObject);
    }

    public void SetLightTrigger(LightTrigger lightTrigger)
    {
        LightTrigger= lightTrigger;
    }

    public void LightActivated()
    {
        HandleTorchRemoved();
    }

    public void LightChanged()
    {
      
    }

    public void LightDeactivated()
    {
       
    }
}
