using Assets.Scripts.Cosimo.Inventory;
using Codice.Client.Common.GameUI;
using UnityEngine;
using UnityEngine.Tilemaps;

internal class GrabCharacterState : IStateCollision2D
{ 
    // TODO: Need to be more precise. 
    private Player _owner;
    private PlayerController _ownerController;
    private GameObject _torch;
    private Tilemap _tilemap;
    private Animator _animator;
    private float _timer;

    public GrabCharacterState(Player player, PlayerController controller, GameObject torch, Tilemap tilemap,Animator animator)
    {
        _owner= player;
        _ownerController = controller;
        _torch = torch;
        _tilemap = tilemap;
        _animator=animator;
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        
    }

    public void OnCollisionStay2D(Collision2D collision)
    {

    }

    public void OnEnd()
    {
        _timer = 0;
    }

    public void OnFixedUpdate()
    {
    
    }

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
        else
        {
            Vector3 interactionPos = _owner.transform.position;
            targetCellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);
            itemToPick = PlacementManager.Instance.GetItemAt(targetCellPos);
        }

        if (itemToPick != null)
        {
            _owner.Animator.Play(_owner.GrabSettings.clipName);

            TorchType typeToReturn = isMagicalRecall ? TorchType.Magical : TorchType.Normal;

            InventoryManager.Instance.ReturnTorch(typeToReturn);
            PlacementManager.Instance.UnregisterItem(targetCellPos);
            GameObject.Destroy(itemToPick);
            Debug.Log($"[Grab] Raccolta torcia {typeToReturn} dalla cella {targetCellPos}. Contatore aggiornato!");
            return;
        }
       

        

        if(_owner.DetectedObject!=null && _owner.DetectedObject.TryGetComponent<PowderColorChooser>(out var powderData))
        {
            _owner.Animator.Play(_owner.GrabSettings.clipName);

            PowderColor color = powderData.Color;

            InventoryManager.Instance.AddPowder(color, 1);

            GameObject.Destroy(_owner.DetectedObject);
            _owner.DetectedObject=null;
            return;
        }

        _owner.SetState(ECharacterStates.Idle);

    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        
    }

    public void OnUpdate()
    {
        _timer += Time.deltaTime;

        if(_timer >= _owner.GrabSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}