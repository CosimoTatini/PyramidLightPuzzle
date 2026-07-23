using System.Collections.Generic;
using UnityEngine;

public class InteractableContextRegistry
{
    private Dictionary<string, PriorityInteractableSet> _registry = new();

    public PriorityInteractableSet GetOrCreatePriorityInteractableSet(string actionGuid)
    {
        if (!_registry.TryGetValue(actionGuid, out var set))
        {
            set = new();
            _registry[actionGuid] = set;
        }

        return set;
    }

    public PriorityInteractableSet TryGetPriorityInteractableSet(string actionGuid)
    {
        if (_registry.TryGetValue(actionGuid, out var set))
        {
            return set;    
        }
        
        return null;
    }

    public bool RemovePriorityInteractableSet(string actionGuid)
    {
        return _registry.Remove(actionGuid);
    }
}
