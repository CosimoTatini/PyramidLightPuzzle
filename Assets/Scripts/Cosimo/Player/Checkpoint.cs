using UnityEngine;

/// <summary>
/// Checkpoint identification
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [ColorUsage(true, true)]
    [SerializeField] private Color _onSpriteColor = Color.lightGreen;
    [SerializeField] private Color _offSpriteColor = Color.darkSeaGreen;
    [SerializeField] private TriggerArea2D _triggerArea2D;

    public TriggerArea2D TriggerArea2D => _triggerArea2D;

    public void TurnOn()
    {
        if(_spriteRenderer == null) return;
        _spriteRenderer.color = _onSpriteColor;
    }

    public void TurnOff()
    {
        if(_spriteRenderer == null) return;
        _spriteRenderer.color = _offSpriteColor;
    }
}