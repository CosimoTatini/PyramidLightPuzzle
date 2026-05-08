using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Tilemap _tilemap;
    private GameObject _torch;
    private Animator _animator;
    private float _timer;
    public PlaceCharacterState(Player player,PlayerController controller, Tilemap tilemap, GameObject torch,Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _tilemap = tilemap;
        _torch = torch;
        _animator = animator;

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
        _ownerController.Rb.linearVelocity = Vector2.zero;
        Vector2 look = _ownerController.LastLookDirection;
        _owner.Animator.SetFloat("MoveX", look.x);
        _owner.Animator.SetFloat("MoveY", look.y);
        PlaceTorchAttempt();
    }
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
            _owner.Animator.Play(_owner.PlaceSettings.clipName);
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
        _timer += Time.deltaTime;
        
        if(_timer >= _owner.PlaceSettings.clip.length)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }


}