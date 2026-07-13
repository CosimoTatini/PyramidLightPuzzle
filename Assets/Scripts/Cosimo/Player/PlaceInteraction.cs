using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceInteraction : MonoBehaviour, IPriorityInteractable
{
    [SerializeField] private Player _player;
    [field: SerializeField] public InputConfigSO InputConfigSO { get; }

    private void Awake()
    {
        _player.AddInteractionEntry(this);
    }

    public InputActionEntry GetFirstEntry()
    {
        if (InputConfigSO == null) return null;

        var allConfigActionsGuids = InputConfigSO.GetInputAssetMaps()
        .SelectMany(k => k.InputMapStructs)
        .SelectMany(k => k.InputActionEntries);

        if (allConfigActionsGuids != null && allConfigActionsGuids.Count() > 0)
        {
            return allConfigActionsGuids.ElementAt(0);
        }
        else
        {
            return null;
        }
    }

    public void Interact()
    {
        _player.SetState(ECharacterStates.Place);
    }
}
