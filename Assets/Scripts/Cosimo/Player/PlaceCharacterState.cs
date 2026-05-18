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

        // Get cell
        // Check singleton for cell availability
        // if true place, else not

        Vector3 targetWorldPos = _owner.transform.position + (Vector3)_ownerController.LastLookDirection;
        Vector3Int cellPos = _tilemap.WorldToCell(targetWorldPos);

        cellPos.z = 0;

        if (_tilemap.HasTile(cellPos))
        {
            Item[] allItems = GameObject.FindObjectsByType<Item>(FindObjectsSortMode.None);
            bool isOccupied = false;
            int torchLayer = LayerMask.NameToLayer("Torch");

            foreach (var item in allItems)
            {
                if (item.gameObject.layer == torchLayer)
                {
                    Vector3Int torchCell = _tilemap.WorldToCell(item.transform.position);
                    torchCell.z = 0; 
                    Debug.Log($"Controllo: Cella Target {cellPos} vs Cella Oggetto {torchCell}");

                    if (torchCell.x == cellPos.x && torchCell.y == cellPos.y && torchCell.z==cellPos.z)
                    {
                        isOccupied = true;
                        break;
                    }
                }
            }

            if (!isOccupied)
            {
                Vector3 spawnPos = _tilemap.GetCellCenterWorld(cellPos);
                spawnPos.z = 0;
                GameObject.Instantiate(_torch, spawnPos, Quaternion.identity);
                Debug.Log("Torcia piazzata.");
                _owner.Animator.SetBool("IsPlacing", true);
            }
            else
            {
                Debug.Log("Impossibile piazzare: Cella già occupata.");
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