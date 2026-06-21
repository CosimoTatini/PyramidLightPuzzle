using Codice.Client.Common.GameUI;
using UnityEngine;
using UnityEngine.Tilemaps;

internal class GrabCharacterState : IStateCollision2D
{
   //TODO: when i collect the torch, i can collect it even if the player is a few outside the cell, i can retrieve it if the whole player is fully in the cell
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
    }

    public void OnFixedUpdate()
    {
    
    }

    public void OnStart()
    {
        _timer = 0;
        _ownerController.Rb.linearVelocity = Vector2.zero; 

       
        _owner.Animator.Play(_owner.GrabSettings.clipName);

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
           
            Vector3 interactionPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection * 0.2f;
            targetCellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);

           
            Vector3 cellCenter = _owner.PlaceableTilemap.GetCellCenterWorld(targetCellPos);
            _owner.transform.position = new Vector3(cellCenter.x, cellCenter.y, _owner.transform.position.z);

            
            itemToPick = PlacementManager.Instance.GetItemAt(targetCellPos);
        }

        if (itemToPick != null)
        {
           
            TorchType typeToReturn = isMagicalRecall ? TorchType.Magical : TorchType.Normal;

            
            InventoryManager.Instance.ReturnTorch(typeToReturn);

            
            PlacementManager.Instance.UnregisterItem(targetCellPos);

           
            GameObject.Destroy(itemToPick);

            // Debug.Log($"[Grab] Raccolta torcia {typeToReturn} dalla cella {targetCellPos}. Contatore aggiornato!");
        }
        else
        {
            // Debug.LogWarning($"[Grab] Nessuna torcia trovata per il tipo selezionato: {InventoryManager.Instance.SelectedType}");
        }
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