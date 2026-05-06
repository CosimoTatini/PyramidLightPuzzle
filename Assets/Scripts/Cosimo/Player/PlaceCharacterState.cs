using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Tilemap _tilemap;
    private GameObject _torch;
    public PlaceCharacterState(Player player,PlayerController controller, Tilemap tilemap, GameObject torch)
    {
        _owner = player;
        _ownerController = controller;
        _tilemap = tilemap;
        _torch = torch;

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
       _owner.Animator.SetBool("IsPlacing",false);
    }

    public void OnFixedUpdate()
    {
        
    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;
        Vector2 look = _ownerController.LastLookDirection;
        _owner.Animator.SetFloat("MoveX", look.x);
        _owner.Animator.SetFloat("MoveY", look.y);
        PlaceTorchAttempt();
    }

    // TODO: Singleton PlacementManager
    // Handles tilemap cells status (free or occupied), only tracks the status, doesn't place anything
    // - Has a dictionary<Vector2Int, Item>, (can make it more future-proof with dictionary<tilemap<dictionary<Vector2Int, Item>>)
    // - Has methods to work with the dictionary => Add, Remove
    // - For each tileMap you have a List<Vector2Int> (so a dictionary<tilemap, List>) that prevents the player from interacting, cells can be removed, added from these lists

    private void PlaceTorchAttempt()
    {
        if (_torch == null || _tilemap == null) return;

       
        Vector3 targetWorldPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection;
        Vector3Int cellPos = _tilemap.WorldToCell(targetWorldPos);
        cellPos.z = 0; 
        
        if (!_tilemap.HasTile(cellPos))
        {
            CancelPlacement("Nessun terreno valido qui.");
            return;
        }

      
        if (PlacementManager.Instance.IsCellAvailable(cellPos))
        {
            ExecutePlacement(cellPos);
        }
        else
        {
            Debug.Log($"[Placement] Cella {cellPos} già occupata.");
            _owner.SetState(ECharacterStates.Idle);
        }
    }

    private void ExecutePlacement(Vector3Int cellPos)
    {
       Vector3 spawnPos = _tilemap.GetCellCenterWorld(cellPos);
        spawnPos.z = 0;

        GameObject torchPrefab = GameObject.Instantiate(_torch,spawnPos,Quaternion.identity);

        if (PlacementManager.Instance.IsPossibleToRegisterItem(cellPos, torchPrefab))
        {
            _owner.Animator.SetBool("IsPlacing", true);
            Debug.Log("Torcia piazzata correttamente.");
        }
        else
        {
            GameObject.Destroy(torchPrefab);
        }
    }

    private void CancelPlacement(string v)
    {
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
       // TODO: Player needs animation clips to be passed, so each state that has a exit time (not in the animator) which is the same
       // as the duration of the clip should use the clip's length
       // Also you can go from Walk to Place and viceversa in the animator
        AnimatorStateInfo stateInfo = _owner.Animator.GetCurrentAnimatorStateInfo(0);
        
        if(stateInfo.IsTag("Place") && stateInfo.normalizedTime >= 1f)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }


}