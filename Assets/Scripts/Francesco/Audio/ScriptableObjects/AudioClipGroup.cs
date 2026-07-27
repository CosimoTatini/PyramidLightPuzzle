using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = nameof(AudioClipGroup), menuName = "SFX"
    + "/" + nameof(AudioClipGroup)
    )]

public class AudioClipGroup : ScriptableObject
{
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioMixerGroup _mixerGroup;

    public AudioClip AudioClip { get { return _audioClip; } }
    public AudioMixerGroup MixerGroup { get { return _mixerGroup; } }

}
