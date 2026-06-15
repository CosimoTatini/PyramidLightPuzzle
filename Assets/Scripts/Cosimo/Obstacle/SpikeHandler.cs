using System;
using System.Collections;
using System.Data.SqlTypes;
using UnityEngine;

public class SpikeHandler : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private float _delay;

    public float Delay
    {
        get => _delay;
        set => _delay = value;
    }

    private Renderer _renderer;
    private Collider2D _collider;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>(true);
        _collider = GetComponent<Collider2D>();

        if (_renderer == null) Debug.LogError($"[SpikeHandler] Manca il Renderer su {gameObject.name} o nei figli!", this);
        if (_collider == null) Debug.LogError($"[SpikeHandler] Manca il Collider2D su {gameObject.name}!", this);
    }

    public void ActivateTrap()
    {
        gameObject.SetActive(true);
        if (_renderer != null) _renderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    public void DeactivateTrap()
    {
        // Spegniamo l'intero GameObject. Il Manager sarà comunque in grado 
        // di riattivarlo perché la Coroutine gira sul Manager (Parent), che resta acceso.
        gameObject.SetActive(false);
    }
}
