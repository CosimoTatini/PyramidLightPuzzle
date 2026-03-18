
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    [Header("CheckpointSystem")]
    public List<Transform> CheckPoints = new List<Transform>();
    private Transform _currentCheckpoint;
    
    private void Awake()
    {
        _inventoryManager = FindFirstObjectByType <InventoryManager>();
    }

    private void Start()
    {
        if(CheckPoints.Count > 0)
        {
            _currentCheckpoint= CheckPoints[0];
        }
        transform.position = CheckPoints[0].position;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item>(out Item item ))
        {
            bool added = _inventoryManager.AddItemInInventory(item.ItemName,item.Quantity,item.Sprite,item.ItemDescription);
            Debug.Log(added);
            if (added)
            {
                Destroy(collision.gameObject);
            }

        }

        if(collision.TryGetComponent<Checkpoint>(out Checkpoint checkpoint))
        {
            _currentCheckpoint = checkpoint.transform;

            if(!CheckPoints.Contains(_currentCheckpoint))
            {
                CheckPoints.Add(_currentCheckpoint);
                Debug.Log("Checkpoint reached:" + checkpoint.CheckpointID);
            }
            
        }

        if(collision.TryGetComponent<Obstacle>(out Obstacle obstacle))
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        if(_currentCheckpoint!=null)
        {
            transform.position= _currentCheckpoint.transform.position;
            Debug.Log("Respawn done");
        }

        else
        {
            Debug.LogWarning("No checkpoint saved found");
        }
    }
}
