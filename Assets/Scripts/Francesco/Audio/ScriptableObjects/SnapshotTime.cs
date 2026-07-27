using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = nameof(SnapshotTime), menuName = "SFX"
    + "/" + nameof(SnapshotTime)
    )]

public class SnapshotTime : ScriptableObject
{
    [SerializeField] protected AudioMixerSnapshot _snapshot;
    [SerializeField] protected float _transitionDuration;

    public virtual AudioMixerSnapshot Snapshot { get { return _snapshot; } }
    public virtual float TransitionDuration { get { return _transitionDuration; } }
}
