using System;
using UnityEngine;

namespace DesignPatterns.Generics
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _quitting = false;

    public static T Instance
    {
        get
        {
            if (_quitting) return null; // Prevents "Ghost" singletons during shutdown

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // DontDestroyOnLoad(gameObject); // Optional: depends on your persistence needs
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"Duplicate Singleton {typeof(T).Name} found! Destroying {name}.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit() => _quitting = true;
}
}


