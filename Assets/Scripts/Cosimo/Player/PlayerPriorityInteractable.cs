using UnityEngine;

public abstract class PlayerPriorityInteractable : PriorityInteraction
{
    protected Player _player = null;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public override void Interact()
    {
    }
}
