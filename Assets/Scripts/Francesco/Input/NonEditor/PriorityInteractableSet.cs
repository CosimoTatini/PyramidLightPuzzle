using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityInteractableSet
{
    private Dictionary<int, List<IPriorityInteractable>> _interactablesPriorityDict = new();
    public Dictionary<int, List<IPriorityInteractable>> InteractablesPriorityDict => _interactablesPriorityDict;
    private int? _currentInteractableListKey = null;
    public int? CurrentInteractableListKey => _currentInteractableListKey;

    public bool AddInteractable(IPriorityInteractable interactable)
    {
        if (interactable == null) return false;

        var entry = interactable.GetFirstEntry();
        if (entry == null) return false;

        bool createdNewList = false;
        if (!_interactablesPriorityDict.ContainsKey(entry.Priority))
        {
            _interactablesPriorityDict[entry.Priority] = new();
            createdNewList = true;
        }

        if (_interactablesPriorityDict[entry.Priority].Contains(interactable)) return false;

        _interactablesPriorityDict[entry.Priority].Add(interactable);
        Debug.Log("Added" + interactable);
        if (createdNewList)
        {
            Debug.Log("Add Recalculated key");
            int? highestPriorityKey = _interactablesPriorityDict.Keys.ElementAt(0);
            foreach (var priority in _interactablesPriorityDict.Keys)
            {
                if (priority > highestPriorityKey)
                {
                    highestPriorityKey = priority;
                }
            }
            _currentInteractableListKey = highestPriorityKey;
        }

        return true;
    }
    public bool RemoveInteractable(IPriorityInteractable interactable)
    {
        if (interactable == null) return false;
        //TODO: in player script I can just make a wrapper method Add/Remove so I can do this
        // if (interactable == _currentInteractable)
        // {
        //     InputConfigManager.UnregisterConfig(interactable.InputConfigSO);
        //     _currentInteractable = null;
        // }
        var entry = interactable.GetFirstEntry();
        if (entry == null) return false;
        if (!_interactablesPriorityDict.ContainsKey(entry.Priority)) return false;

        if (!_interactablesPriorityDict[entry.Priority].Contains(interactable)) return false;

        _interactablesPriorityDict[entry.Priority].Remove(item: interactable);
        Debug.Log("REMOVED" + interactable);

        if (_interactablesPriorityDict[entry.Priority].Count == 0)
        {
            Debug.Log("Remove Recalculated key");
            _interactablesPriorityDict.Remove(entry.Priority);
            int? highestPriorityKey = _interactablesPriorityDict.Keys.Count > 0 ? _interactablesPriorityDict.Keys.ElementAt(0) : null;
            foreach (var priority in _interactablesPriorityDict.Keys)
            {
                if (priority > highestPriorityKey)
                {
                    highestPriorityKey = priority;
                }
            }
            _currentInteractableListKey = highestPriorityKey;
        }

        return true;
    }

    public void Clear()
    {
        if (_interactablesPriorityDict != null)
        {
            _interactablesPriorityDict.Clear();
        }
        _currentInteractableListKey = null;
    }
}