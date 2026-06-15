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
            // Forza lo spegnimento iniziale di tutte le trappole prima di avviare il ciclo
            foreach (var spike in _spikeObstacles)
            {
                if (spike != null) spike.DeactivateTrap();
            }

            StartCoroutine(ActivationDeactivationCoroutine());
        }
        else
        {
            Debug.LogError($"[SpikeManager] ERRORE: Nessuno SpikeHandler trovato sotto {gameObject.name}.", this);
        }
    }

    private void InitializeCache()
    {
        _spikeObstacles.Clear();

        // Trova tutti i componenti SpikeHandler nei figli, inclusi quelli disattivati (true)
        SpikeHandler[] handlers = GetComponentsInChildren<SpikeHandler>(true);

        if (handlers != null && handlers.Length > 0)
        {
            _spikeObstacles.AddRange(handlers);

            // Ordina la lista in base al Delay impostato dalla tua finestra Editor 🛠️
            _spikeObstacles.Sort((x, y) => x.Delay.CompareTo(y.Delay));
            Debug.Log($"[SpikeManager] Cache configurata con successo. {_spikeObstacles.Count} spine pronte.", this);
        }

        _waitBeforeDeactivate = new WaitForSeconds(_delayBeforeDeactivation);
        _waitRestart = new WaitForSeconds(_delayBeforeLoopRestart);
    }

    private IEnumerator ActivationDeactivationCoroutine()
    {
        // Aspetta che Unity completi l'inizializzazione del frame iniziale
        yield return new WaitForEndOfFrame();

        while (true)
        {
            Debug.Log("[SpikeManager] --- Inizio Fase Attivazione ---");

            // FASE 1: ATTIVAZIONE IN SEQUENZA
            for (int i = 0; i < _spikeObstacles.Count; i++)
            {
                if (_spikeObstacles[i] == null) continue;

                float waitTime = _spikeObstacles[i].Delay;
                if (i > 0)
                {
                    waitTime = _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay;
                }

                if (waitTime > 0f)
                {
                    yield return new WaitForSeconds(waitTime);
                }

                _spikeObstacles[i].ActivateTrap();
                Debug.Log($"[SpikeManager] Spina {i} ACCESA a schermo (Delay: {_spikeObstacles[i].Delay}s)");
            }

            Debug.Log("[SpikeManager] Tutte le spine fuori. Aspetto la disattivazione...");
            yield return _waitBeforeDeactivate;

            Debug.Log("[SpikeManager] --- Inizio Fase Disattivazione (Dalla prima) ---");

            // FASE 2: DISATTIVAZIONE IN SEQUENZA
            for (int i = 0; i < _spikeObstacles.Count; i++)
            {
                if (_spikeObstacles[i] == null) continue;

                float waitTime = _spikeObstacles[i].Delay;
                if (i > 0)
                {
                    waitTime = _spikeObstacles[i].Delay - _spikeObstacles[i - 1].Delay;
                }

                if (waitTime > 0f)
                {
                    yield return new WaitForSeconds(waitTime);
                }

                _spikeObstacles[i].DeactivateTrap();
                Debug.Log($"[SpikeManager] Spina {i} SPENTA a schermo (Delay: {_spikeObstacles[i].Delay}s)");
            }

            Debug.Log("[SpikeManager] Ciclo terminato. Aspetto il riavvio...");
            yield return _waitRestart;
        }
    }
}
