using System;
using System.Collections;
using System.Data.SqlTypes;
using UnityEngine;

public class SpikeHandler : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private AnimSettings _animSettings;

    [Header("Spike Config")]
    [SerializeField] private float _delay;

    public float Delay
    {
        get => _delay;
        set => _delay = value;
    }

    private Renderer _renderer;
    private Collider2D _collider;
    private Animator _animator;

    private void Awake()
    {
        // Cerca i componenti nei figli o sul parent in modo sicuro
        _renderer = GetComponentInChildren<Renderer>(true);
        _collider = GetComponent<Collider2D>();
        _animator = GetComponentInChildren<Animator>(true);

        if (_renderer == null) Debug.LogError($"[SpikeHandler] Manca Renderer su {gameObject.name}!", this);
        if (_collider == null) Debug.LogError($"[SpikeHandler] Manca Collider2D su {gameObject.name}!", this);
        if (_animator == null) Debug.LogError($"[SpikeHandler] Manca Animator su {gameObject.name} o nei figli!", this);

        // Stato iniziale: spento
        SetSpikeState(false);
    }

    public void ActivateTrap()
    {
        SetSpikeState(true);

        if (_animator != null && _animSettings != null)
        {
            _animator.speed = 1f; // Riproduzione normale
            _animator.Play(_animSettings.clipName, 0, 0f);
        }
    }

    public IEnumerator DeactivateTrapCoroutine()
    {
        if (_animator != null && _animSettings != null)
        {
            _animator.speed = -1f; 
            _animator.Play(_animSettings.clipName, 0, 1f); 

            
            float clipLength = _animSettings.clip != null ? _animSettings.clip.length : 1f;
            yield return new WaitForSeconds(clipLength);
        }

        SetSpikeState(false);
    }

    private void SetSpikeState(bool isActive)
    {
        if (_renderer != null) _renderer.enabled = isActive;
        if (_collider != null) _collider.enabled = isActive;
    }
}
