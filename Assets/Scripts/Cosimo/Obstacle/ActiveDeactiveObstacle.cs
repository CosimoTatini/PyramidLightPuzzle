using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveDeactiveObstacle : MonoBehaviour
{
    //[Header("Global Timing Settings")]
    //[SerializeField] private float _delayBeforeDeactivation = 2.0f;
    //[SerializeField] private float _delayBeforeLoopRestart = 3.0f;

    //private List<SpikeHandler> _spikeObstacles = new List<SpikeHandler>();
    //private WaitForSeconds _waitBeforeDeactivate;
    //private WaitForSeconds _waitRestart;

    //private void Start()
    //{
    //    InitializeCache();

    //    if (_spikeObstacles.Count > 0)
    //    {
    //        StartCoroutine(ActivationDeactivationCoroutine());
    //    }
    //    else
    //    {
    //        Debug.LogError($"[SpikeManager] Nessuno SpikeHandler valido trovato sotto {gameObject.name}.", this);
    //    }
    //}

    //private void InitializeCache()
    //{
    //    _spikeObstacles.Clear();
    //    SpikeHandler[] handlers = GetComponentsInChildren<SpikeHandler>(true);

    //    if (handlers != null && handlers.Length > 0)
    //    {
    //        _spikeObstacles.AddRange(handlers);
    //        // Ordina la lista in base ai delay calcolati dalla finestra Editor Custom
    //        _spikeObstacles.Sort((x, y) => x.Delay.CompareTo(y.Delay));
    //    }

    //    _waitBeforeDeactivate = new WaitForSeconds(_delayBeforeDeactivation);
    //    _waitRestart = new WaitForSeconds(_delayBeforeLoopRestart);
    //}

    //private IEnumerator ActivationDeactivationCoroutine()
    //{
    //    // Attesa di sicurezza iniziale
    //    yield return new WaitForEndOfFrame();

    //    while (true)
    //    {
    //        // FASE 1: ATTIVAZIONE IN ONDA (In Avanti)
    //        for (int i = 0; i < _spikeObstacles.Count; i++)
    //        {
    //            if (_spikeObstacles[i] == null) continue;

    //            float waitTime = i > 0 ? _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay : _spikeObstacles[i].Delay;
    //            if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

    //            _spikeObstacles[i].ActivateTrap();
    //        }

    //        // Aspetta il tempo in cui le spine rimangono totalmente estratte
    //        yield return _waitBeforeDeactivate;

    //        // FASE 2: DISATTIVAZIONE IN ONDA (Sempre dalla prima, con Rewind)
    //        for (int i = 0; i < _spikeObstacles.Count; i++)
    //        {
    //            if (_spikeObstacles[i] == null) continue;

    //            float waitTime = i > 0 ? _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay : _spikeObstacles[i].Delay;
    //            if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

    //            // Eseguiamo il rewind in secondo piano per ogni spina
    //            StartCoroutine(_spikeObstacles[i].DeactivateTrapCoroutine());
    //        }

    //        // Aspetta il delay di fine ciclo prima di far ripartire l'intera sequenza
    //        yield return _waitRestart;
    //    }
    //}
}