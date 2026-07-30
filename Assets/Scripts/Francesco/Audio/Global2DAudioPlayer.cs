using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

// TODO, Can't be a Singleton since for now the class Destroys the instances to allow Reloading correctly
public class Global2DAudioPlayer : MonoBehaviour
{
    #region Singleton

    private static Global2DAudioPlayer _instance;

    public static Global2DAudioPlayer Instance
    {
        get
        {
            if (_instance) return _instance;

            _instance = FindFirstObjectByType<Global2DAudioPlayer>(FindObjectsInactive.Include);

            if (_instance) return _instance;

            return Instantiate(Resources.Load<Global2DAudioPlayer>(nameof(Global2DAudioPlayer)), Vector3.zero, Quaternion.identity).GetComponent<Global2DAudioPlayer>();
        }
        set
        {
            _instance = value;
        }
    }

    #endregion

    [SerializeField] private AudioMixer _audioMixer;

    private Dictionary<AudioMixerGroup, AudioSource> _groupSource;

    public Dictionary<AudioMixerGroup, AudioSource> GroupSource
    {
        get { return _groupSource; }
    }

    public static UnityAction<AudioClip, AudioMixerGroup> OnPlayAudioClip;
    public static UnityAction<AudioClipGroup> OnPlayClipGroup;
    public static UnityAction<AudioClipListGroup> OnPlayRandomClip;
    public static UnityAction<AudioClip[], AudioMixerGroup> OnPlayRandomClipGroup;

    public static UnityAction<AudioClip, AudioMixerGroup> OnPlayLoopAudioClip;
    public static UnityAction<AudioClipGroup> OnPlayLoopClipGroup;
    public static UnityAction<AudioClip[], AudioMixerGroup> OnPlayLoopRandomClip;
    public static UnityAction<AudioClip[], AudioMixerGroup> OnPlayLoopRandomClipGroup;

    private ObjectPooler<AudioSource> _audioSourcePooler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AudioSource audioSource = new();
        _audioSourcePooler = new(audioSource,true);
        DontDestroyOnLoad(gameObject);

        LoadAudioGroupsSources();

        //AddListeners();
    }

    private void OnDestroy()
    {
        //RemoveListeners();
    }

    private void AddListeners()
    {
        OnPlayAudioClip += InternalPlayOneShot;
        OnPlayClipGroup += InternalPlayOneShot;
        OnPlayRandomClip += InternalPlayOneShotRandom;
        OnPlayRandomClipGroup += InternalPlayOneShotRandom;

        OnPlayLoopAudioClip += InternalPlayLoop;
        OnPlayLoopClipGroup += InternalPlayLoop;
        OnPlayLoopRandomClip += InternalPlayLoopRandom;
        OnPlayLoopRandomClipGroup += InternalPlayLoopRandom;
    }

    private void RemoveListeners()
    {
        OnPlayAudioClip -= InternalPlayOneShot;
        OnPlayClipGroup -= InternalPlayOneShot;
        OnPlayRandomClip -= InternalPlayOneShotRandom;
        OnPlayRandomClipGroup -= InternalPlayOneShotRandom;

        OnPlayLoopAudioClip -= InternalPlayLoop;
        OnPlayLoopClipGroup -= InternalPlayLoop;
        OnPlayLoopRandomClip -= InternalPlayLoopRandom;
        OnPlayLoopRandomClipGroup -= InternalPlayLoopRandom;
    }

    // Fills the dictionary of AudioMixerGroups and AudioSources
    // by getting all of the groups from the AudioMixer,
    // then creating an AudioSource for each of the mixers and assigning it
    // the mixer through which they should output the sound
    private void LoadAudioGroupsSources()
    {
        _groupSource = new();

        AudioMixerGroup[] audioMixerGroups = _audioMixer.FindMatchingGroups("");

        for (int i = 0; i < audioMixerGroups.Length; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = audioMixerGroups[i];

            _groupSource[audioMixerGroups[i]] = audioSource;
        }
    }

    public void PlayOneShot(AudioClipGroup clipGroup)
    {
        InternalPlayOneShot(clipGroup.AudioClip, clipGroup.MixerGroup);
    }

    public void PlayOneShot(AudioClip audioClip, AudioMixerGroup group)
    {
        InternalPlayOneShot(audioClip, group);
    }

    public void PlayOneShotRandom(AudioClipListGroup clipListGroup)
    {
        InternalPlayOneShotRandom(clipListGroup);
    }

    public void PlayOneShotRandom(IEnumerable<AudioClip> clips, AudioMixerGroup group)
    {
        InternalPlayOneShotRandom(clips, group);
    }

    public void PlayLoop(AudioClipGroup clipGroup)
    {
        InternalPlayLoop(clipGroup);
    }

    public void PlayLoop(AudioClip audioClip, AudioMixerGroup group)
    {
        InternalPlayLoop(audioClip, group);
    }

    public void PlayLoopRandom(AudioClipListGroup clipListGroup)
    {
        InternalPlayLoopRandom(clipListGroup);
    }

    public void PlayLoopRandom(IEnumerable<AudioClip> clips, AudioMixerGroup group)
    {
        InternalPlayLoopRandom(clips, group);
    }

    private void InternalPlayOneShot(AudioClip audioClip, AudioMixerGroup group)
    {
        // Check audioClip and group validity
        if (!IsAudioClipGroupValid(audioClip, group))
            return;

        AudioSource audioSource = _groupSource[group];
        audioSource.PlayOneShot(audioClip);
    }

    private void InternalPlayOneShot(AudioClipGroup clipGroup)
    {
        InternalPlayOneShot(clipGroup.AudioClip, clipGroup.MixerGroup);
    }

    private void InternalPlayLoop(AudioClip audioClip, AudioMixerGroup group)
    {
        // Check audioClip and group validity
        if (!IsAudioClipGroupValid(audioClip, group))
            return;

        AudioSource audioSource = _groupSource[group];
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void InternalPlayLoop(AudioClipGroup clipGroup)
    {
        InternalPlayLoop(clipGroup.AudioClip, clipGroup.MixerGroup);
    }

    private void InternalPlayOneShotRandom(IEnumerable<AudioClip> clips, AudioMixerGroup group)
    {
        var invalidClips = clips
            .Where(clip => !IsAudioClipGroupValid(clip, group));

        if (invalidClips.Any())
            return;

        int soundIndex = Random.Range(0, clips.Count());

        AudioSource audioSource = _groupSource[group];
        audioSource.PlayOneShot(clips.ElementAt(soundIndex));
    }

    private void InternalPlayOneShotRandom(AudioClipListGroup clipsGroup)
    {
        InternalPlayOneShotRandom(clipsGroup.AudioClips, clipsGroup.MixerGroup);
    }

    private void InternalPlayLoopRandom(IEnumerable<AudioClip> clips, AudioMixerGroup group)
    {
        var invalidClips = clips
            .Where(clip => !IsAudioClipGroupValid(clip, group));

        if (invalidClips.Any())
            return;

        int soundIndex = Random.Range(0, clips.Count());

        AudioSource audioSource = _groupSource[group];
        audioSource.clip = clips.ElementAt(soundIndex);
        audioSource.loop = true;
        audioSource.Play();
    }

    private void InternalPlayLoopRandom(AudioClipListGroup clipsGroup)
    {
        InternalPlayLoopRandom(clipsGroup.AudioClips, clipsGroup.MixerGroup);
    }

    

    private bool IsAudioClipGroupValid(AudioClipGroup clipGroup)
    {
        if (!clipGroup.AudioClip || !clipGroup.MixerGroup)
        {
            string errorMessage = "Make sure clip group: " + clipGroup.name + "'s";

            if (!clipGroup.AudioClip)
            {
                errorMessage += " audioClip";
                if (!clipGroup.MixerGroup)
                {
                    errorMessage += " and audioMixerGroup are valid";
                    Debug.LogError(errorMessage);
                    return false;
                }
            }
            else
            {
                errorMessage += " audioMixerGroup";
            }

            errorMessage += " is valid";

            Debug.LogError(errorMessage);
            return false;
        }

        return true;
    }

    private bool IsAudioClipGroupValid(AudioClip audioClip, AudioMixerGroup group)
    {
        if (!audioClip || !group)
        {
            string errorMessage = "Make sure";

            if (!audioClip)
            {
                errorMessage += " audioClip";
                if (!group)
                {
                    errorMessage += " and audioMixerGroup are valid";
                    Debug.LogError(errorMessage);
                    return false;
                }
            }
            else
            {
                errorMessage += " audioMixerGroup";
            }

            errorMessage += " is valid";

            Debug.LogError(errorMessage);
            return false;
        }

        return true;
    }

    public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float transitionTime)
    {
        snapshot.TransitionTo(transitionTime);
    }

    public void TransitionToSnapshot(SnapshotTime snapshotTime)
    {
        snapshotTime.Snapshot.TransitionTo(snapshotTime.TransitionDuration);
    }
}
