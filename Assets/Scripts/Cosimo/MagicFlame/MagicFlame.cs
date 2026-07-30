using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MagicFlame : MonoBehaviour
{
    [SerializeField] private LightEmitter _lightEmitter;


    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if(_lightEmitter != null )
        {
            _lightEmitter= GetComponentInParent<LightEmitter>();
        }
    }

    private void OnEnable()
    {
        if(_lightEmitter!=null)
        {
            _lightEmitter.OnLightChanged += UpdateFlameColor;
        }
    }
    private void OnDisable()
    {
        if( _lightEmitter!=null)
        {
            _lightEmitter.OnLightChanged -= UpdateFlameColor;
        }
    }
    private void UpdateFlameColor(LightEmitter emitter)
    {
        if (_spriteRenderer == null) return;

        if (emitter.Light != null && !emitter.Light.enabled)
        {
            _spriteRenderer.enabled = false;
            return;
        }
        _spriteRenderer.enabled = true;

        if (emitter.Light != null)
        {
            _spriteRenderer.color = emitter.Light.color;
        }
    }
}
