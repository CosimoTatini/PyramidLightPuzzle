using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestisce la riproduzione temporizzata dei versi della Mummia.
/// </summary>
public class MummyGrowler : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent _onGrowl;

    [Header("Timer Settings")]
    [SerializeField] private float _baseInterval = 3f;
    [SerializeField] private float _minReduction = 1f;
    [SerializeField] private float _maxReduction = 2f;

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
            
            float randomReduction = Random.Range(_minReduction, _maxReduction);

         
            float actualWaitTime = Mathf.Max(0.1f, _baseInterval - randomReduction);

           
            yield return new WaitForSeconds(actualWaitTime);

         
            _onGrowl?.Invoke();
        }
    }
}