using UnityEngine;
using UnityEngine.Events;

public class LastDoor : MonoBehaviour
{
    [SerializeField] private BigFlame[] _bigFlames;
    [SerializeField] private Animator _animator;

    private bool _isDoorOpen = false;
    public bool IsDoorOpen => _isDoorOpen;

    public UnityEvent OnDoorOpen;
    public UnityEvent OnAllFlamesActive;

    void OnEnable()
    {
        for (int i = 0; i < _bigFlames.Length; i++)
        {
            BigFlame bigFlame = _bigFlames[i];
            bigFlame.OnFlameOn.RemoveListener(InvokeAllFlamesActive);
            bigFlame.OnFlameOn.AddListener(InvokeAllFlamesActive);
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < _bigFlames.Length; i++)
        {
            BigFlame bigFlame = _bigFlames[i];
            bigFlame.OnFlameOn.RemoveListener(InvokeAllFlamesActive);
        }
    }

    public bool AreAllFlamesActive()
    {
        for (int i = 0; i < _bigFlames.Length; i++)
        {
            BigFlame bigFlame = _bigFlames[i];
            if (!bigFlame.IsOn)
            {
                return false;
            }
        }

        return true;
    }

    private void InvokeAllFlamesActive()
    {
        if (AreAllFlamesActive()) OnAllFlamesActive.Invoke();
    }

    public void OpenDoorIfAllFlamesActive()
    {
        if (!AreAllFlamesActive()) return;

        OpenDoor();
    }

    public void OpenDoor()
    {
        if (_isDoorOpen) return;
        _isDoorOpen = true;
        _animator.Play("OpenDoor");
        OnDoorOpen.Invoke();
    }
}
