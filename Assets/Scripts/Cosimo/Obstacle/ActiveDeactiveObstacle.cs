using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveDeactiveObstacle : MonoBehaviour
{
    [Header("Global Timing Settings")]
    [SerializeField] private float _delayBeforeDeactivation = 2.0f;
    [SerializeField] private float _delayBeforeLoopRestart = 3.0f;

    private List<SpikeHandler> _spikeObstacles = new List<SpikeHandler>();
    private WaitForSeconds _waitBeforeDeactivate;
    private WaitForSeconds _waitRestart;

    private void Start()
    {
        InitializeCache();

        if (_spikeObstacles.Count > 0)
        {
            StartCoroutine(ActivationDeactivationCoroutine());
        }
        else
        {
            Debug.LogError($"[SpikeManager] Nessuno SpikeHandler valido trovato sotto {gameObject.name}.", this);
        }
    }

    private void InitializeCache()
    {
        _spikeObstacles.Clear();
        SpikeHandler[] handlers = GetComponentsInChildren<SpikeHandler>(true);

        if (handlers != null && handlers.Length > 0)
        {
            _spikeObstacles.AddRange(handlers);
            // Ordina la lista in base ai dati della finestra Editor Custom
            _spikeObstacles.Sort((x, y) => x.Delay.CompareTo(y.Delay));
        }

        _waitBeforeDeactivate = new WaitForSeconds(_delayBeforeDeactivation);
        _waitRestart = new WaitForSeconds(_delayBeforeLoopRestart);
    }

    private IEnumerator ActivationDeactivationCoroutine()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            // 1. FASE DI ATTIVAZIONE SEQUENZIALE
            for (int i = 0; i < _spikeObstacles.Count; i++)
            {
                if (_spikeObstacles[i] == null) continue;

                float waitTime = i > 0 ? _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay : _spikeObstacles[i].Delay;
                if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

                _spikeObstacles[i].ActivateTrap();
            }

            // Attesa con tutte le spine fuori e attive
            yield return _waitBeforeDeactivate;

            // 2. FASE DI DISATTIVAZIONE SEQUENZIALE (Rewind + Spegnimento)
            for (int i = 0; i < _spikeObstacles.Count; i++)
            {
                if (_spikeObstacles[i] == null) continue;

                float waitTime = i > 0 ? _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay : _spikeObstacles[i].Delay;
                if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

                // Lanciamo la coroutine di disattivazione sulla singola spina così il rewind 
                // avviene in secondo piano senza bloccare il ciclo sequenziale del Manager! 🚀
                StartCoroutine(_spikeObstacles[i].DeactivateTrapCoroutine());
            }

            // Attesa prima di ricominciare il ciclo dell'ostacolo
            yield return _waitRestart;
        }
    }
}
