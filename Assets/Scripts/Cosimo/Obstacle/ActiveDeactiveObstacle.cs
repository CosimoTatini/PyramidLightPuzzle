using System;
using System.Collections;
using UnityEngine;

public class ActiveDeactiveObstacle : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float _timeBetweenActivation = 1.0f;
    [SerializeField] private float _delayBeforeDeactivation = 2.0f;
    [SerializeField] private float _timeBetweenDeactivation = 1.0f;
    [SerializeField] private float _delayBeforeLoopRestart = 3.0f; // Il tempo X di attesa prima di ripartire

    // Cache per ottimizzare le prestazioni ed evitare il GetChild nel ciclo
    private Transform[] _childObstacles;

    private void Start()
    {
        InitializeCache();

        if (_childObstacles.Length > 0)
        {
            StartCoroutine(ActivationDeactivationCoroutine());
        }
    }

    private void InitializeCache()
    {
        int childCount = transform.childCount;
        _childObstacles = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _childObstacles[i] = transform.GetChild(i);
        }
    }

    private IEnumerator ActivationDeactivationCoroutine()
    {
        
        while (true)
        {
            
            for (int i = 0; i < _childObstacles.Length; i++)
            {
                _childObstacles[i].gameObject.SetActive(true);

                // Nota: Quando inserirai l'Animator, potrai fare qualcosa del genere:
                // if (_childObstacles[i].TryGetComponent(out Animator anim)) anim.Play("YourClipName");

                yield return new WaitForSeconds(_timeBetweenActivation);
            }

           
            yield return new WaitForSeconds(_delayBeforeDeactivation);

           
            for (int i = _childObstacles.Length - 1; i >= 0; i--)
            {
                _childObstacles[i].gameObject.SetActive(false);
                yield return new WaitForSeconds(_timeBetweenDeactivation);
            }

           
            yield return new WaitForSeconds(_delayBeforeLoopRestart);
        }
    }
}
