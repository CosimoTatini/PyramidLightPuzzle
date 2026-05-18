using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProximityTooltip : MonoBehaviour
{
    [Tooltip("Used for distance check")]
    [SerializeField] private Transform _checkTransform;
    private InteractionTooltip _previousTooltip;
    private InteractionTooltip _currentTooltip;
    private List<InteractionTooltip> _detectedTooltips = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out InteractionTooltip tooltip))
        {
            _detectedTooltips.Add(tooltip);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out InteractionTooltip tooltip))
        {
            _previousTooltip = _currentTooltip;
            _currentTooltip = GetClosestTooltip();
            if (_currentTooltip != _previousTooltip)
            {
                _previousTooltip?.Hide();
                _currentTooltip?.Show(); 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)  
    {
        if (collision.TryGetComponent(out InteractionTooltip tooltip))
        {
            if (_currentTooltip && _currentTooltip == tooltip)
            { 
                tooltip.Hide();
            }
            _detectedTooltips.Remove(tooltip);
            if (_detectedTooltips.Count == 0)
            { 
                _currentTooltip = null;
                _previousTooltip = null;
            }
        }
    }

    private InteractionTooltip GetClosestTooltip()
    {
        return _detectedTooltips.OrderBy(ttip => Vector2.Distance(ttip.transform.position, _checkTransform.position)).FirstOrDefault();
    }
}
