using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System;

[InitializeOnLoad]
public static class InputConfigPriorityCache
{
    // No need to unsubscribe anywhere since it's going to on each reload
    static InputConfigPriorityCache()
    {
        Undo.undoRedoPerformed -= RebuildPriorityDictionary;
        Undo.undoRedoPerformed += RebuildPriorityDictionary;

        EditorApplication.projectChanged -= RebuildPriorityDictionary;
        EditorApplication.projectChanged += RebuildPriorityDictionary;

        RebuildPriorityDictionary();
    }

    public static Action OnRebuildCompleted;

    private static Dictionary<string, HashSet<InputConfigSO>> _actionsPriorities = new();
    // <guid,<priority, priorityCount>>
    private static Dictionary<string, Dictionary<int, int>> _actionsPrioritiesCount = new();

    public static Dictionary<string, HashSet<InputConfigSO>> ActionsPriorities => _actionsPriorities;
    public static Dictionary<string, Dictionary<int, int>> ActionsPrioritiesCount => _actionsPrioritiesCount;

    private static bool _duplicatePrioritySearchActive;

    public static bool DuplicatePrioritySearchActive => _duplicatePrioritySearchActive;

    private static EditorCoroutine DuplicatePriorityResearch;

    public static void RebuildPriorityDictionary()
    {
        // _activeInstances.RemoveWhere(instance => instance == null);

        // Debug.Log("Rebuilding");
        // if (_activeInstances.Count == 0) return;
        if (DuplicatePriorityResearch != null)
        {
            EditorCoroutineUtility.StopCoroutine(DuplicatePriorityResearch);
            DuplicatePriorityResearch = null;
        }
        // Debug.Log("Rebuilding " + _activeInstances.ElementAt(0).target.name);

        _duplicatePrioritySearchActive = true;
        DuplicatePriorityResearch = EditorCoroutineUtility.StartCoroutineOwnerless(DuplicatePrioritySearchCoroutine());
    }

    private static IEnumerator DuplicatePrioritySearchCoroutine()
    {
        // use temporary dictionaries so we don't leave a window where there is no data to read
        Dictionary<string, HashSet<InputConfigSO>> actionPrioritiesTemp = new();
        Dictionary<string, Dictionary<int, int>> actionPrioritiesCountTemp = new();

        var allConfigs = AssetDatabaseUtils.GetAssetsByType<InputConfigSO>();

        // find all configs where action is found
        for (int i = allConfigs.Length - 1; i >= 0; i--)
        {
            // bool foundAction = false;
            InputConfigSO config = allConfigs[i];

            var inputAssetMapLists = config.GetInputAssetMaps();

            for (int j = 0; j < inputAssetMapLists.Count; j++)
            {
                var inputMapStructs = inputAssetMapLists[j].InputMapStructs;
                for (int k = 0; k < inputMapStructs.Count; k++)
                {
                    var inputActionStructs = inputMapStructs[k].InputActionStructs;
                    for (int l = 0; l < inputActionStructs.Count; l++)
                    {
                        InputActionStruct inputActionStruct = inputActionStructs[l];
                        string guid = inputActionStruct.Guid;
                        int priority = inputActionStruct.Priority;

                        // build priority dictionary
                        if (!actionPrioritiesCountTemp.ContainsKey(guid)) actionPrioritiesCountTemp[guid] = new();
                        var priorityCount = actionPrioritiesCountTemp[guid];
                        if (!priorityCount.ContainsKey(priority)) priorityCount[priority] = 0;
                        priorityCount[priority]++;

                        if (!actionPrioritiesTemp.ContainsKey(guid)) actionPrioritiesTemp[guid] = new();
                        var inputConfigs = actionPrioritiesTemp[guid];
                        inputConfigs.Add(config);
                    }
                }
            }
        }

        // when completed update the dictionaries with the fresh data
        _actionsPriorities = actionPrioritiesTemp;
        _actionsPrioritiesCount = actionPrioritiesCountTemp;

        // List<InputConfigSOEditor> inputConfigSOEditors = _activeInstances.ToList();
        // for (int i = inputConfigSOEditors.Count - 1; i >= 0; i--)
        // {
        //     inputConfigSOEditors?[i].Repaint();
        // }
        _duplicatePrioritySearchActive = false;
        OnRebuildCompleted?.Invoke();
        yield break;
    }

    public static bool IsThereAnyPriorityConflict(string guid)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return false;

        foreach (var priorityCounts in _actionsPrioritiesCount[guid])
        {
            if (priorityCounts.Value > 1)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetPriorityCountForAction(string guid, int priority)
    {
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return -1;
        if (!_actionsPrioritiesCount[guid].ContainsKey(priority)) return -1;

        return _actionsPrioritiesCount[guid][priority];
    }

    public static PriorityAvailabilityEnum IsPriorityAvailable(string guid, int requestedPriority)
    {
        if (_actionsPrioritiesCount.Count == 0) return PriorityAvailabilityEnum.NO_CONFLICT;
        if (!_actionsPrioritiesCount.ContainsKey(guid)) return PriorityAvailabilityEnum.NO_CONFLICT;

        int priorityCount = GetPriorityCountForAction(guid, requestedPriority);
        bool isThereAnyPriorityConflict = IsThereAnyPriorityConflict(guid);
        if (priorityCount == -1 || priorityCount - 1 > 0)
        {
            return PriorityAvailabilityEnum.SELF_CONFLICT;
        }
        else
        {
            if (isThereAnyPriorityConflict)
            {
                return PriorityAvailabilityEnum.SELF_AVAILABLE;
            }
            else
            {
                return PriorityAvailabilityEnum.NO_CONFLICT;
            }
        }
    }

    public enum PriorityAvailabilityEnum
    {
        /// <summary>
        /// Selected priority doesn't conflict, but there are there configs that do
        /// </summary>
        SELF_AVAILABLE,
        /// <summary>
        /// Selected priority conflicts with at least another config
        /// </summary>
        SELF_CONFLICT,
        /// <summary>
        /// All of the configs don't conflict with each other
        /// </summary>
        NO_CONFLICT
    }
}