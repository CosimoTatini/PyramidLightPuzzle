using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private Checkpoint[] _checkpoints;
    private Checkpoint _currentCheckpoint = null;
    public Checkpoint CurrentCheckpoint => _currentCheckpoint;
    private List<Action<Collider2D>> _checkpointsHandlers = new();

    void OnEnable()
    {
        _checkpoints = GetComponentsInChildren<Checkpoint>();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            Checkpoint checkpoint = _checkpoints[i];
            if (checkpoint.TriggerArea2D != null)
            {
                Action<Collider2D> checkpointHandler = (collision) => OnCheckpointTriggered(checkpoint, collision);
                checkpoint.TriggerArea2D.OnTriggerEnterAction += checkpointHandler;
                _checkpointsHandlers.Add(checkpointHandler);
            }
        }
    }

    void OnDisable()
    {
        for (int i = _checkpoints.Length - 1; i >= 0; i--)
        {
            Checkpoint checkpoint = _checkpoints[i];
            if (checkpoint.TriggerArea2D != null)
            {
                checkpoint.TriggerArea2D.OnTriggerEnterAction -= _checkpointsHandlers[i];
            }
        }
    }

    private void OnCheckpointTriggered(Checkpoint checkpoint, Collider2D collision)
    {
        if (collision == null || checkpoint == null) return;

        if (!collision.TryGetComponent(out Player player)) return;

        Checkpoint previousCheckpoint = _currentCheckpoint;

        if (previousCheckpoint != null)
        {
            if (previousCheckpoint != checkpoint)
            {
                previousCheckpoint.TurnOff();
            }
        }
        _currentCheckpoint = checkpoint;
        player.CurrentCheckPoint = _currentCheckpoint;
        _currentCheckpoint.TurnOn();
    }
}
