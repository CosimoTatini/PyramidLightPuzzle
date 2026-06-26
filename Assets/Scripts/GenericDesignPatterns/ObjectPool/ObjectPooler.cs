using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPooler<T> where T : Component // Changed to Component so we can easily access .gameObject and .transform
{
    private T monoBehaviourPrefab;
    private Queue<T> objectPool;
    private Transform poolRoot; // A hidden container to store inactive objects

    public ObjectPooler(T prefab, string poolName = "ObjectPool")
    {
        objectPool = new Queue<T>();
        monoBehaviourPrefab = prefab;

        // Create a hidden, persistent root object in the scene to hold pooled items
        GameObject rootGo = new GameObject($"[Pool] {poolName}");
        // Object.DontDestroyOnLoad(rootGo);
        poolRoot = rootGo.transform;
        rootGo.SetActive(false); // Turning off the root automatically turns off all its children!
    }

    public T Get(Transform newParent = null)
    {
        T obj;

        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
        }
        else
        {
            if (monoBehaviourPrefab == null)
                throw new NullReferenceException("Non è stato inizializzato il monoBehaviourPrefab.");

            obj = Object.Instantiate(monoBehaviourPrefab);
        }

        // Move it to its new UI parent and wake it up
        obj.transform.SetParent(newParent, false);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Set(T monoBehaviour)
    {
        if (monoBehaviour == null) return;

        // 🏠 Reparent it to the hidden pool root. 
        // Because the poolRoot GameObject is inactive, this automatically disables the row!
        monoBehaviour.transform.SetParent(poolRoot, false);

        objectPool.Enqueue(monoBehaviour);
    }
}