using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceInteraction : PlayerPriorityInteractable
{
    public override void Interact()
    {
        if(_player == null) return;
        _player.SetState(ECharacterStates.Place);
    }
}
