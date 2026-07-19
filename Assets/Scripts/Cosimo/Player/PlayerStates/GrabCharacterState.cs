using Assets.Scripts.Cosimo.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// State to handle the grab interactions
/// </summary>
internal class GrabCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private GameObject _torch;
    private Tilemap _tilemap;
    private Animator _animator;
    private float _timer;

    // Ottimizzazione allocazione: Array statico riutilizzabile per evitare Garbage Collection in OnStart
    private static readonly Vector3Int[] CardinalDirections = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    public GrabCharacterState(Player player, PlayerController controller, GameObject torch, Tilemap tilemap, Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _torch = torch;
        _tilemap = tilemap;
        _animator = animator;
    }

    public void OnCollisionEnter2D(Collision2D collision) { }
    public void OnCollisionExit2D(Collision2D collision) { }
    public void OnCollisionStay2D(Collision2D collision) { }

    public void OnEnd()
    {
        _timer = 0;
    }

    public void OnFixedUpdate() { }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;

        // GameObject itemToPick = null;
        // Vector3Int targetCellPos = Vector3Int.zero;
        // bool isMagicalRecall = InventoryManager.Instance.SelectedType == TorchType.Magical;

        // if (isMagicalRecall)
        // {
        //     var magicalTorchData = PlacementManager.Instance.FindMagicalTorch();
        //     if (magicalTorchData != null)
        //     {
        //         itemToPick = magicalTorchData;
        //         // targetCellPos = magicalTorchData.Value.Key;
        //     }
        // }


        // if (itemToPick == null)
        // {
        //     Tilemap groundTilemap = _owner.PlaceableTilemap;


        //     Vector3 feetPosition = _owner.FeetTransform.position;
        //     Vector3Int feetCellPos = groundTilemap.WorldToCell(feetPosition);
        //     GameObject torchUnderFeet = PlacementManager.Instance.GetItemAt(feetCellPos);


        //     bool hasFreeAdjacentCell = false;
        //     for (int i = 0; i < CardinalDirections.Length; i++)
        //     {
        //         Vector3Int checkCell = feetCellPos + CardinalDirections[i];

        //         if (groundTilemap.HasTile(checkCell) && PlacementManager.Instance.IsCellAvailable(groundTilemap, checkCell))
        //         {
        //             hasFreeAdjacentCell = true;
        //             break;
        //         }
        //     }

        //     if (torchUnderFeet != null && (hasFreeAdjacentCell || itemToPick == null))
        //     {
        //         itemToPick = torchUnderFeet;
        //         targetCellPos = feetCellPos;
        //     }
        // }

        // if (itemToPick != null)
        // {
        //     if (itemToPick.TryGetComponent<TypeChooser>(out var torchComponent))
        //     {
        //         if (torchComponent.Type == InventoryManager.Instance.SelectedType)
        //         {
        //             _owner.Animator.Play(_owner.GrabSettings.clipName);

        //             if (torchComponent.Type == TorchType.Magical)
        //             {
        //                 if (itemToPick.TryGetComponent<LightEmitter>(out var lightEmitter))
        //                 {
        //                     RecoverPowder(lightEmitter);
        //                 }
        //             }

        //             InventoryManager.Instance.ReturnTorch(torchComponent.Type);


        //             PlacementManager.Instance.TryToUnregisterItem(targetCellPos);

        //             if (torchComponent.IsEternal)
        //             {
        //                 torchComponent.IsEternal = false;
        //                 Debug.Log($"[Grab] Torcia eterna rilevata. Evento lanciato e flag resettato a false.");
        //             }

        //             GameObject.Destroy(itemToPick);
        //             Debug.Log($"[Grab] Raccolta torcia {torchComponent.Type} dalla cella {targetCellPos}. Contatore aggiornato!");
        //             return;
        //         }
        //         else
        //         {
        //             itemToPick = null;
        //         }
        //     }
        //     else
        //     {
        //         itemToPick = null;
        //     }
        // }

        // if (_owner.DetectedObject != null && _owner.DetectedObject.TryGetComponent<PowderColorChooser>(out var powderData))
        // {
        //     _owner.Animator.Play(_owner.GrabSettings.clipName);

        //     PowderColor color = powderData.Color;
        //     InventoryManager.Instance.AddPowder(color, 1);

        //     GameObject.Destroy(_owner.DetectedObject);
        //     _owner.DetectedObject = null;
        //     return;
        // }

        // _owner.SetState(ECharacterStates.Idle);
        _owner.Animator.Play(_owner.GrabSettings.clipName);
    }

    private void RecoverPowder(LightEmitter lightEmitter)
    {
        if (lightEmitter.RedAmount > 0)
        {
            InventoryManager.Instance.AddPowder(PowderColor.Red, lightEmitter.RedAmount);
        }

        if (lightEmitter.GreenAmount > 0)
        {
            InventoryManager.Instance.AddPowder(PowderColor.Green, lightEmitter.GreenAmount);
        }

        if (lightEmitter.BlueAmount > 0)
        {
            InventoryManager.Instance.AddPowder(PowderColor.Blue, lightEmitter.BlueAmount);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider) { }
    public void OnTriggerExit2D(Collider2D collider) { }
    public void OnTriggerStay2D(Collider2D collider) { }

    public void OnUpdate()
    {
        _timer += Time.deltaTime;

        if (_timer >= _owner.GrabSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}