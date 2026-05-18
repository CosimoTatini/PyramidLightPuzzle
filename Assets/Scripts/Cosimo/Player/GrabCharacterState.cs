using Codice.Client.Common.GameUI;
using UnityEngine;
using UnityEngine.Tilemaps;

internal class GrabCharacterState : IStateCollision2D
{
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

        Vector3 interactionPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection * 0.2f;
        Vector3Int cellPos = _owner.PlaceableTilemap.WorldToCell(interactionPos);

        Vector3 cellCenter = _owner.PlaceableTilemap.GetCellCenterWorld(cellPos);
        _owner.transform.position = new Vector3(cellCenter.x, cellCenter.y, _owner.transform.position.z);

        _owner.Animator.Play(_owner.GrabSettings.clipName);

        GameObject itemToPick = PlacementManager.Instance.GetItemAt(cellPos);

        if (itemToPick != null)
        {
         
            bool isMagical = itemToPick.GetComponentInChildren<MagicalTorch>() != null;
            TorchType torchType = isMagical ? TorchType.Magical : TorchType.Normal;
            InventoryManager.Instance.ReturnTorch(torchType);

            PlacementManager.Instance.UnregisterItem(cellPos);
            GameObject.Destroy(itemToPick);

            Debug.Log($"[Grab] Raccolta torcia identificata come: {torchType}. Contatore HUD aggiornato!");
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