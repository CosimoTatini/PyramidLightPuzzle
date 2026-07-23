using log4net.Util;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementInteraction : MonoBehaviour, IPriorityInteractable
{
    [SerializeField] private Player _player;
    [field: SerializeField] public InputConfigSO InputConfigSO { get; }

    private void Awake()
    {
        _player.AddInteractionEntry(this);
        Debug.Log("Awake eseguito!");
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
        TorchType selectedType = InventoryManager.Instance.SelectedType;
        Debug.Log($"[PlacementInteraction] Interazione avviata. Torcia selezionata nell'inventario: {selectedType}");

        // 1. Controllo Torcia Magica
        if (selectedType == TorchType.Magical)
        {
            bool hasMagicalTorch = PlacementManager.Instance.FindMagicalTorch().HasValue;
            Debug.Log($"[PlacementInteraction] Controllo Torcia Magica posizionata: {hasMagicalTorch}");

            if (hasMagicalTorch)
            {
                Debug.Log("[PlacementInteraction] -> Transizione a Grab (Torcia Magica trovata)");
                _player.SetState(ECharacterStates.Grab);
                return;
            }
        }

        Tilemap placeableTilemap = PlacementManager.Instance.TargetTilemap;

        if (placeableTilemap == null)
        {
            Debug.LogError("[PlacementInteraction] TargetTilemap è null su PlacementManager!");
            return;
        }

        // 2. Controllo Cella Corrente sotto il Player
        Vector3Int currentCellPos = placeableTilemap.WorldToCell(_player.transform.position);
        bool isCurrentAvailable = PlacementManager.Instance.IsCellAvailable(placeableTilemap, currentCellPos);
        GameObject currentItem = PlacementManager.Instance.GetItemAt(currentCellPos);

        Debug.Log($"[PlacementInteraction] Cella Player: {currentCellPos} | Disponibile: {isCurrentAvailable} | Oggetto presente: {(currentItem != null ? currentItem.name : "Nessuno")}");

        if (!isCurrentAvailable)
        {
            if (selectedType == TorchType.Normal)
            {
                Debug.Log("[PlacementInteraction] -> Transizione a Grab (Cella del player occupata + Torcia Normale)");
                _player.SetState(ECharacterStates.Grab);
                return;
            }
            else
            {
                Debug.LogWarning($"[PlacementInteraction] Cella occupata, ma il tipo selezionato è '{selectedType}' e non 'Normal'.");
            }
        }

        // 3. Controllo Cella Frontale rispetto alla direzione del Player
        Vector3 targetWorldPos = _player.transform.position + (Vector3)_player.PlayerController.LastLookDirection * _player.CellOffset;
        Vector3Int forwardCellPos = placeableTilemap.WorldToCell(targetWorldPos);
        bool isForwardAvailable = PlacementManager.Instance.IsCellAvailable(placeableTilemap, forwardCellPos);
        GameObject forwardItem = PlacementManager.Instance.GetItemAt(forwardCellPos);

        Debug.Log($"[PlacementInteraction] Cella Frontale: {forwardCellPos} | Disponibile: {isForwardAvailable} | Oggetto presente: {(forwardItem != null ? forwardItem.name : "Nessuno")}");

        if (!isForwardAvailable)
        {
            Vector3 cellCenter = placeableTilemap.GetCellCenterWorld(forwardCellPos);
            float distance = Vector2.Distance(_player.transform.position, cellCenter);

            Debug.Log($"[PlacementInteraction] Calcolo Distanza dal centro cella frontale: {distance:F2} / Soglia: 0.60");

            if (distance <= 0.6f)
            {
                Debug.Log("[PlacementInteraction] -> Transizione a Grab (Cella frontale occupata + Distanza valida)");
                _player.SetState(ECharacterStates.Grab);
                return;
            }
            else
            {
                Debug.LogWarning($"[PlacementInteraction] Troppo lontano dalla cella frontale occupata ({distance:F2} > 0.60).");
            }
        }

        Debug.Log("[PlacementInteraction] Nessuna condizione per il Grab è stata soddisfatta.");
    }
}