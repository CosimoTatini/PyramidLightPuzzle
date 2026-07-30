using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestisce la riproduzione temporizzata e casuale dei versi della Mummia.
/// </summary>
public class MummyGrowler : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent _onGrowl;

    [Header("Timer Settings (in secondi)")]
    [SerializeField] private float _minInterval = 10f;
    [SerializeField] private float _maxInterval = 13f;

    private Coroutine _growlCoroutine;

    private void OnEnable()
    {
        _growlCoroutine = StartCoroutine(GrowlRoutine());
    }

    private void OnDisable()
    {
        if (_growlCoroutine != null)
        {
            StopCoroutine(_growlCoroutine);
            _growlCoroutine = null;
        }
    }

    private IEnumerator GrowlRoutine()
    {
        while (true)
        {
            
            float waitTime = Random.Range(_minInterval, _maxInterval);
            yield return new WaitForSeconds(waitTime);

           
            _onGrowl?.Invoke();
        }
    }
}