using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = nameof(AudioClipListGroup), menuName = "SFX"
    + "/" + nameof(AudioClipListGroup)
    )]
public class AudioClipListGroup : ScriptableObject
{
    [SerializeField] private List<AudioClip> _audioClips;
    [SerializeField] private AudioMixerGroup _mixerGroup;

    public List<AudioClip> AudioClips => _audioClips;
    public AudioMixerGroup MixerGroup => _mixerGroup;
}
