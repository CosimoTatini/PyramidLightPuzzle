using UnityEngine;

public class SpikesSynchronizer : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    [SerializeField] private TimerEvents _timerEventsActivateTrap;
    [SerializeField] private AnimationClip _sampleClipForTiming;
    private SpikeHandler[] _spikeHandlers;

    void Awake()
    {
        _spikeHandlers = GetComponentsInChildren<SpikeHandler>();
        _timerEventsActivateTrap.SecondsToElapse = _sampleClipForTiming.length;
    }

    public void PlayTrapsAnimations()
    {
        for (int i = 0; i < _spikeHandlers.Length; i++)
        {
            SpikeHandler spikeHandler = _spikeHandlers[i];
            spikeHandler.PlayTrapAnimation();
        }
    }

    public void DeactivateTraps()
    {
        for (int i = 0; i < _spikeHandlers.Length; i++)
        {
            SpikeHandler spikeHandler = _spikeHandlers[i];
            spikeHandler.DeactivateTrap();
        }
    }
}