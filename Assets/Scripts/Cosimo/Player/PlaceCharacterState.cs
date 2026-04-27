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
        _owner.Animator.SetBool("IsPlacing", true);
        PlaceTorchAttempt();
    }

    private void PlaceTorchAttempt()
    {
        if (_torch == null || _tilemap == null) return;

        Vector3 targetWorldPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection;
        Vector3Int cellPos = _tilemap.WorldToCell(targetWorldPos);

        if (_tilemap.HasTile(cellPos))
        {
            Vector3 spawnPos = _tilemap.GetCellCenterWorld(cellPos);
            int torchLayer = LayerMask.GetMask("Torch");
            Collider2D hit = Physics2D.OverlapPoint(spawnPos, torchLayer);

            if (hit == null)
            {
                GameObject.Instantiate(_torch, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.Log("Piazzamento bloccato da: " + hit.name);
            }
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
       
        AnimatorStateInfo stateInfo = _owner.Animator.GetCurrentAnimatorStateInfo(0);
        
        if(stateInfo.IsTag("Place") && stateInfo.normalizedTime >= 1f)
        {
            _owner.SetState(ECharacterStates.Idle);
        }
    }
}