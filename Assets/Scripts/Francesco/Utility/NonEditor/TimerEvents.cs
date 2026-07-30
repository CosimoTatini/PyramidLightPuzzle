using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimerEvents : MonoBehaviour
{
    [SerializeField] private float _secondsToElapse;
    [SerializeField] private bool _beginOnStart = false;
    [SerializeField] private float _beginOnStartSecondsDelay = 0f;
    [SerializeField] private bool _shouldLoop = false;
    [SerializeField] private float _secondsBeforeNextLoopCycle;
    [SerializeField] private UnityEvent _onTimerStarted;
    [SerializeField] private UnityEvent _onTimerFinished;
    [SerializeField] private UnityEvent _onTimerInterrupted;
    // [SerializeField] private UnityEvent OnTimerStopped;
    // [SerializeField] private UnityEvent OnTimerResumed;

    public float SecondsToElapse
    {
        get
        {
            return _secondsToElapse;
        }
        set
        {
            if (value < 0f) value = 0f;
            _secondsToElapse = value;
        }
    }

    public UnityEvent OnTimerStarted => _onTimerStarted;
    public UnityEvent OnTimerFinished => _onTimerFinished;
    public UnityEvent OnTimerInterrupted => _onTimerInterrupted;

    private Coroutine _timerCoroutine;
    private bool _isRunning = false;
    // private bool _isTimerStopped = false;

    // private float _timerTimeStart;
    // private float _remainingTime = -1f;

    void Start()
    {
        if (_beginOnStart)
        {
            Invoke(nameof(StartTimer), _beginOnStartSecondsDelay);
        }
    }

    private IEnumerator TimerCoroutine()
    {
        do
        {
            _isRunning = true;
            float timeToElapse;
            _onTimerStarted.Invoke();
            timeToElapse = _secondsToElapse;
            yield return new WaitForSeconds(timeToElapse);

            _onTimerFinished.Invoke();
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
            _onTimerInterrupted.Invoke();
        }

        _isRunning = true;
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    [ContextMenu("InterruptTimer")]
    public void InterruptTimer()
    {
        if (!_isRunning && !_shouldLoop)
        {
            return;
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        _isRunning = false;
        _onTimerInterrupted.Invoke();
    }

    // public void SetUp(float secondsToElapse, bool beginOnStart = false, float beginOnStartSecondsDelay = 0f, bool shouldLoop = false, float secondsBeforeNextLoopCycle = 0f)
    // {

    // }


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
