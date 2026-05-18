using TMPro;
using UnityEngine;
public class InteractionTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textRef;
    [SerializeField] private TextMeshProUGUI _keyTextRef;
    [SerializeField] private GameObject _parent;

    private bool _isActive;
    private string _text;
    private string _keyText;

    public bool IsActive { get { return _isActive; } }
    public string Text { get { return _text; } }

    public string KeyText { get { return _keyText; } }

    public void SetText(string text)
    {
        _text = text;
        _textRef.text = text;
    }

    public void SetKeyText(string keyText)
    {
        _keyText = keyText;
        _keyTextRef.text = keyText;
    }

    public void Show()
    {
        if (_isActive) return;
        _isActive = true;
        _parent.SetActive(true);
    }

    public void Hide()
    {
        if (!IsActive) return;
        _isActive = false;
        _parent.SetActive(false);
    }
}
