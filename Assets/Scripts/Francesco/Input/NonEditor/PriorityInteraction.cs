using System.Linq;
using UnityEngine;

public abstract class PriorityInteraction : MonoBehaviour, IPriorityInteractable
{
    [field: SerializeField] public InputConfigSO InputConfigSO { get; set; }

    public virtual InputActionEntry GetFirstEntry()
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

    public abstract void Interact();
}