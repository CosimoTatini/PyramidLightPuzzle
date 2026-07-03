using Assets.Scripts.Cosimo.Inventory;
using Codice.Client.Common.GameUI;
using System;
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

        GameObject itemToPick = null;
        Vector3Int targetCellPos = Vector3Int.zero;
        bool isMagicalRecall = (InventoryManager.Instance.SelectedType == TorchType.Magical);

        if (isMagicalRecall)
        {
            var magicalTorchData = PlacementManager.Instance.FindMagicalTorch();
            if (magicalTorchData.HasValue)
            {
                itemToPick = magicalTorchData.Value.Value;
                targetCellPos = magicalTorchData.Value.Key;
            }
        }

        if (itemToPick == null)
        {
            Vector3 interactionPos = _owner.transform.position;
            targetCellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);
            itemToPick = PlacementManager.Instance.GetItemAt(targetCellPos);
        }

        if (itemToPick != null)
        {
            if (itemToPick.TryGetComponent<TypeChooser>(out var torchComponent))
            {
                if (torchComponent.Type == InventoryManager.Instance.SelectedType)
                {
                    _owner.Animator.Play(_owner.GrabSettings.clipName);

                    if (torchComponent.Type == TorchType.Magical)
                    {
                        if (itemToPick.TryGetComponent<LightEmitter>(out var lightEmitter))
                        {
                            RecoverPowder(lightEmitter);
                        }
                    }

                    InventoryManager.Instance.ReturnTorch(torchComponent.Type);

                    // 🔔 1. Chiamiamo PRIMA il manager: leggerà la torcia come eterna e lancerà l'evento!
                    PlacementManager.Instance.UnregisterItem(targetCellPos);

                    // 🌟 2. DOPO azzeriamo il flag IsEternal sul componente prima che venga distrutto
                    if (torchComponent.IsEternal)
                    {
                        torchComponent.IsEternal = false;
                        Debug.Log($"[Grab] Torcia eterna rilevata. Evento lanciato e flag resettato a false.");
                    }

                    // ❌ 3. Infine, eliminiamo l'oggetto dalla scena
                    GameObject.Destroy(itemToPick);
                    Debug.Log($"[Grab] Raccolta torcia {torchComponent.Type} dalla cella {targetCellPos}. Contatore aggiornato!");
                    return;
                }
                else
                {
                    itemToPick = null;
                }
            }
            else
            {
                itemToPick = null;
            }
        }

        if (_owner.DetectedObject != null && _owner.DetectedObject.TryGetComponent<PowderColorChooser>(out var powderData))
        {
            _owner.Animator.Play(_owner.GrabSettings.clipName);

            PowderColor color = powderData.Color;
            InventoryManager.Instance.AddPowder(color, 1);

            GameObject.Destroy(_owner.DetectedObject);
            _owner.DetectedObject = null;
            return;
        }

        _owner.SetState(ECharacterStates.Idle);
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
        float durationThreshold = _owner.GrabSettings.clip.length - 0.05f;

        if (_timer >= _owner.GrabSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}