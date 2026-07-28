using UnityEngine;
using UnityEngine.Events;

public class BigFlame : MonoBehaviour
{
    [SerializeField] private GameObject _wholeFlame;
    [SerializeField] private bool _isOn = true;
    public bool IsOn => _isOn;

    public UnityEvent OnFlameOn;
    public UnityEvent OnFlameOff;

    void Start()
    {
        if (_isOn)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    public void TurnOn()
    {
        _wholeFlame.SetActive(true);
        _isOn = true;
        OnFlameOn.Invoke();
    }

    public void TurnOff()
    {
        _wholeFlame.SetActive(false);
        _isOn = false;
        OnFlameOff.Invoke();
    }
}
