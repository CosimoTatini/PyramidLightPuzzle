using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimerEvents : MonoBehaviour
{
    [SerializeField] private float _secondsToElapse;
    [SerializeField] private bool _beginOnStart = false;
    [SerializeField] private bool _shouldLoop = false;
    [SerializeField] private float _secondsBeforeNextLoopCycle;
    [SerializeField] private UnityEvent OnTimerStarted;
    [SerializeField] private UnityEvent OnTimerFinished;
    [SerializeField] private UnityEvent OnTimerInterrupted;
    // [SerializeField] private UnityEvent OnTimerStopped;
    // [SerializeField] private UnityEvent OnTimerResumed;

    private Coroutine _timerCoroutine;
    private bool _isRunning = false;
    // private bool _isTimerStopped = false;

    // private float _timerTimeStart;
    // private float _remainingTime = -1f;

    void Start()
    {
        if (_beginOnStart)
        {
            StartTimer();
        }
    }

    private IEnumerator TimerCoroutine()
    {
        do
        {
            _isRunning = true;
            float timeToElapse;
            OnTimerStarted.Invoke();
            timeToElapse = _secondsToElapse;
            yield return new WaitForSeconds(timeToElapse);

            OnTimerFinished.Invoke();
            _isRunning = false;

            if (_shouldLoop)
            {
                yield return new WaitForSeconds(_secondsBeforeNextLoopCycle);
            }
        } while (_shouldLoop);
    }

    [ContextMenu("StartTimer")]
    public void StartTimer()
    {
        if (_isRunning)
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
                _isRunning = false;
            }
            OnTimerInterrupted.Invoke();
        }

        _isRunning = true;
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    [ContextMenu("InterruptTimer")]
    public void InterruptTimer()
    {
        if (!_isRunning)
        {
            return;
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        _isRunning = false;
        OnTimerInterrupted.Invoke();
    }

    // public void StopTimer()
    // {
    //     if (!_isRunning || _isTimerStopped)
    //     {
    //         return;
    //     }

    //     _isRunning = false;
    //     if (_timerCoroutine != null)
    //     {
    //         StopCoroutine(_timerCoroutine);
    //         _timerCoroutine = null;
    //     }

    //     if (_remainingTime >= 0.00001f)
    //     {
    //         OnTimerStopped.Invoke();
    //     }
    //     else if (_remainingTime == -1f)
    //     {
    //         _remainingTime = 0f;
    //         OnTimerFinished.Invoke();
    //     }
    //     else
    //     {

    //     }
    // }

    // public void ResumeTimer()
    // {
    //     if (_isRunning)
    //     {
    //         return;
    //     }

    //     if (_timerCoroutine != null)
    //     {
    //         StopCoroutine(_timerCoroutine);
    //         _timerCoroutine = null;
    //     }
    //     _isRunning = true;
    //     _timerCoroutine = StartCoroutine(TimerCoroutine());
    // }
}
