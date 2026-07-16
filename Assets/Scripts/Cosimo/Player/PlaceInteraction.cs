using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceInteraction : PriorityInteractable
{
    [SerializeField] private Player _player;
    private void Awake()
    {
        _player.AddInteractionEntry(this);
    }

    public override void Interact()
    {
        _player.SetState(ECharacterStates.Place);
    }
}
