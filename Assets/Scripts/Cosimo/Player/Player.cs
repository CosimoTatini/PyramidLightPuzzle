
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public class Player : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    [Header("CheckpointSystem")]
    public List<Transform> CheckPoints = new List<Transform>();
    private Transform _currentCheckpoint;
    [SerializeField] private float _fallDuration = 0.5f;
    [SerializeField] private AnimationCurve _fallCurve;
    private bool _isRespawning;
    
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
            if(!_isRespawning)
            {
                StartCoroutine(FallAndRespawnCoroutine());
            }
            
;        }
    }

    private IEnumerator FallAndRespawnCoroutine()
    {
        _isRespawning = true;

        float timer = 0f;
        Vector3 startScale = transform.localScale;
        while (timer < _fallDuration)
        {
            timer += Time.deltaTime;
            float t = timer / _fallDuration;

            float scale = Mathf.Lerp(0.5f,1f,_fallCurve.Evaluate(t));

            transform.localScale = new Vector3(scale,scale,scale);
            yield return null;

        }
        Respawn();
        transform.localScale = startScale;
        _isRespawning=false;
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
