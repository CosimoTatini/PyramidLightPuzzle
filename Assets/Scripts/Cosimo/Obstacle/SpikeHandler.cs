using System;
using System.Collections;
using System.Data.SqlTypes;
using UnityEngine;

public class SpikeHandler : MonoBehaviour
{
    [SerializeField] private float _timeBeetweenSpikes = 1.0f;
    [SerializeField] private float _activeDuration = 2.0f;
    [SerializeField] private float _inactiveDuration = 2.0f;
    [SerializeField] private float _delay;
    public float Delay
    {
        get => _delay;
        set => _delay = value;
    }
    private void Start()
    {
        int index = transform.GetSiblingIndex();
        _delay = index * _timeBeetweenSpikes;
        StartCoroutine(SpikeLifecyleCoroutine());
    }

    private IEnumerator SpikeLifecyleCoroutine()
    {
       yield return new WaitForSeconds(_delay);

        while(true)
        {
            ToggleSpikeVisual(true);
            yield return new WaitForSeconds(_activeDuration);

            ToggleSpikeVisual(false);
            yield return new WaitForSeconds(_inactiveDuration);
        }
    }

    private void ToggleSpikeVisual(bool isActive)
    {
        if(TryGetComponent(out Renderer renderer))
        {
            renderer.enabled = isActive;
        }

        if(TryGetComponent(out Collider2D collider))
        {
            collider.enabled = isActive;
        }
    }
}
