using System;
using System.Collections;
using UnityEngine;

public class SpikeHandler : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip _animationClip;

    public void PlayTrapAnimation()
    {
        _animator.SetFloat("Speed", 1);
        _animator.Play(_animationClip.name, 0, 0f);
    }

    public void DeactivateTrap()
    {
        _animator.SetFloat("Speed", -1);
        _animator.Play(_animationClip.name, 0, 1f);
    }
    // [Header("Animation Settings")]
    // [SerializeField] private AnimSettings _animSettings;

    // [Header("Spike Config")]
    // [SerializeField] private float _delay;

    // public float Delay
    // {
    //    get => _delay;
    //    set => _delay = value;
    // }

    // //private Renderer _renderer;
    // private Collider2D _collider;
    // private Animator _animator;

    // private void Awake()
    // {
    //    //_renderer = GetComponentInChildren<Renderer>(true);
    //    _collider = GetComponent<Collider2D>();
    //    _animator = GetComponentInChildren<Animator>(true);

    //    //if (_renderer == null) Debug.LogError($"[SpikeHandler] Manca Renderer su {gameObject.name}!", this);
    //    if (_collider == null) Debug.LogError($"[SpikeHandler] Manca Collider2D su {gameObject.name}!", this);
    //    if (_animator == null) Debug.LogError($"[SpikeHandler] Manca Animator su {gameObject.name} o nei figli!", this);

    //    // Stato iniziale: spento
    //    SetSpikeState(false);
    // }

    // public void ActivateTrap()
    // {
    //    SetSpikeState(true);

    //    if (_animator != null && _animSettings != null)
    //    {
    //        gameObject.SetActive(true);
    //        _animator.speed = 1f; // Velocità normale avanti

    //        // Forza la clip a ripartire esattamente dal frame 0 (normalizedTime = 0f)
    //        _animator.Play(_animSettings.clipName, 0, 0f);
    //    }
    // }

    // public IEnumerator DeactivateTrapCoroutine()
    // {
    //    if (_animator != null && _animSettings != null)
    //    {
    //        _animator.speed = -1f; // Imposta la velocità in negativo per il Rewind ⏪

    //        // Forza l'animazione a posizionarsi all'ultimo frame (normalizedTime = 1f)
    //        // prima di iniziare a riprodurre all'indietro!
    //        _animator.Play(_animSettings.clipName, 0, 1f);

    //        // Aspetta la durata della clip prima di spegnere i componenti hardware
    //        float clipLength = _animSettings.clip != null ? _animSettings.clip.length : 1f;
    //        yield return new WaitForSeconds(clipLength);
    //    }

    //    SetSpikeState(false);
    // }

    // private void SetSpikeState(bool isActive)
    // {
    //    //if (_renderer != null) _renderer.enabled = isActive;
    //    if (_collider != null) _collider.enabled = isActive;
    // }
}
