using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private Tilemap _tilemap;
    private GameObject _torchPrefab;
    private Animator _animator;
    private float _timer;
    private float _torchDuration = 30f;
    public PlaceCharacterState(Player player,PlayerController controller, Tilemap tilemap, GameObject torch,Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _tilemap = tilemap;
        _torchPrefab = torch;
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
        _timer = 0;
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

        if (!InventoryManager.Instance.CanPlace()) 
        {
            Debug.LogWarning("[PlaceState] Torce esaurite nel manager!"); 
            _owner.SetState(ECharacterStates.Idle); 
            return; 
        }

        Tilemap groundTilemap = _owner.PlaceableTilemap; 

        if (groundTilemap == null) 
        {
            Debug.LogError("[PlaceState] La PlaceableTilemap sul Player non è assegnata nell'Inspector!"); 
            _owner.SetState(ECharacterStates.Idle); 
            return; 
        }

        Vector3 interactionPos = _owner.transform.position + (Vector3)look * 0.8f; 
        Vector3Int cellPos = groundTilemap.WorldToCell(interactionPos); 

        if (!groundTilemap.HasTile(cellPos)) 
        {
            Debug.LogWarning($"[PlaceState] Impossibile piazzare: Non c'è terreno nella cella {cellPos} della Tilemap Ground!"); 
            _owner.SetState(ECharacterStates.Idle); 
            return; 
        }

    
        if (!PlacementManager.Instance.IsCellAvailable(groundTilemap, cellPos)) 
        {
            Debug.LogWarning($"[PlaceState] Piazzamento impedito: la cella {cellPos} è ristretta o già occupata!");
            _owner.SetState(ECharacterStates.Idle);
            return; 
        }

      
        Vector3 spawnWorldPos = groundTilemap.GetCellCenterWorld(cellPos); 

        TorchType type = InventoryManager.Instance.SelectedType; 
        GameObject prefabToSpawn = (type == TorchType.Normal) 
            ? InventoryManager.Instance.TorchPrefab 
            : InventoryManager.Instance.MagicalTorchPrefab; 

        GameObject torchInstance = GameObject.Instantiate(prefabToSpawn, spawnWorldPos, Quaternion.identity); 

        
        if (PlacementManager.Instance.IsPossibleToRegisterItem(groundTilemap, cellPos, torchInstance, type)) 
        {
            _owner.Animator.Play(_owner.PlaceSettings.clipName); 
            InventoryManager.Instance.UseTorch();

            _owner.StartCoroutine(TorchLifetimeCoroutine(torchInstance, type, cellPos)); 
            Debug.Log($"[Place] Torcia di tipo {type} piazzata correttamente sulla Tilemap Ground nella cella: {cellPos}"); 
        }
        else
        {
          
            GameObject.Destroy(torchInstance); 
            _owner.SetState(ECharacterStates.Idle); 
        }
    }

    private IEnumerator TorchLifetimeCoroutine(GameObject torchInstance, TorchType type, Vector3Int cellPos)
    {
        yield return new WaitForSeconds(_torchDuration);

        if (torchInstance != null && type is TorchType.Normal)
        {
            PlacementManager.Instance.UnregisterItem(cellPos);
            GameObject.Destroy(torchInstance);
            InventoryManager.Instance.ReturnTorch(type);
            Debug.Log($"[Lifetime] Torcia scaduta e rimossa dalla cella {cellPos}.");
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

      
        if (_timer >= _owner.PlaceSettings.clip.length)
        {
            
            if (_ownerController.MoveDirection.sqrMagnitude > 0.01f)
            {
                _owner.SetState(ECharacterStates.Walk);
            }
            else
            {
                _owner.SetState(ECharacterStates.Idle);
            }
        }
    }


}