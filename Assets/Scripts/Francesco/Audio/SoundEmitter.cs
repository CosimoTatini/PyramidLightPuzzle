using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private Transform _audioSourcesParent = null;
    private Transform _poolerOneShotParent = null;
    private Transform _poolerLoopParent = null;
    private ObjectPooler<AudioSource> _poolerOneShot;
    private ObjectPooler<AudioSource> _poolerLoop;

    private Dictionary<string, AudioSource> _trackedOneShots;
    private Dictionary<string, AudioSource> _trackedLoops;

    private Dictionary<Guid, Coroutine> _activeReturnAudioSourcesCoroutines;

    void Awake()
    {
        _activeReturnAudioSourcesCoroutines = new();

        if (_audioSourcesParent == null)
        {
            GameObject parent = new("AudioSources");
            parent.transform.SetParent(transform);
            _audioSourcesParent = parent.transform;
            _audioSourcesParent.localPosition = Vector3.zero;
        }

        if (_poolerOneShotParent == null)
        {
            GameObject parent = new("[Pool] OneShot AudioSources");
            parent.transform.SetParent(transform);
            _poolerOneShotParent = parent.transform;
            _poolerOneShotParent.localPosition = Vector3.zero;
        }

        if (_poolerLoopParent == null)
        {
            GameObject parent = new("[Pool] Loop AudioSources");
            parent.transform.SetParent(transform);
            _poolerLoopParent = parent.transform;
            _poolerLoopParent.localPosition = Vector3.zero;
        }

        GameObject audioSourcePrefab = new("AudioSourcePrefab");
        audioSourcePrefab.transform.SetParent(_audioSourcesParent);
        AudioSource audioSource = audioSourcePrefab.AddComponent<AudioSource>();
        _poolerOneShot = new(audioSource, parent: _poolerOneShotParent);
        _poolerLoop = new(audioSource, parent: _poolerLoopParent);

        _poolerOneShot.Set(audioSource);

        _trackedOneShots = new();
        _trackedLoops = new();
    }

    //TODO: I need to set it up so it's reusable for next pool, like if the object itself is a pooled object
    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    void OnDestroy()
    {

    }

    public void PlayOneShot(SoundConfig soundConfig)
    {
        if (soundConfig == null) return;

        AudioSource audioSource = _poolerOneShot.Get(_audioSourcesParent);
        audioSource.transform.localPosition = Vector3.zero;
        soundConfig.ApplyToSource(audioSource);

        audioSource.loop = false;
        audioSource.PlayOneShot(soundConfig.Clip);
        Guid guid = Guid.NewGuid();
        _activeReturnAudioSourcesCoroutines.Add(guid, StartCoroutine(ReturnAudioSourceToPoolAfterPlaying(audioSource, guid)));
    }

    public void PlayOneShotTracked(SoundConfig soundConfig, bool shouldRestartIfPlayingAlready = true)
    {
        if (soundConfig == null) return;

        string id = soundConfig.name;

        if (_trackedOneShots.TryGetValue(id, out var audioSource))
        {
            // if playing and shouldn't restart, then just apply the effect
            if (audioSource.isPlaying)
            {
                if (shouldRestartIfPlayingAlready)
                {
                    audioSource.Stop();
                }
                else
                {
                    soundConfig.ApplyToSource(audioSource);
                    audioSource.transform.localPosition = Vector3.zero;
                    audioSource.loop = false;
                    return;
                }
            }

            soundConfig.ApplyToSource(audioSource);
            audioSource.loop = false;
            audioSource.transform.localPosition = Vector3.zero;
            audioSource.Play();
            return;
        }

        AudioSource newAudioSource = _poolerOneShot.Get(_audioSourcesParent);
        newAudioSource.transform.localPosition = Vector3.zero;

        soundConfig.ApplyToSource(newAudioSource);
        newAudioSource.loop = false;
        newAudioSource.Play();

        _trackedOneShots.Add(id, newAudioSource);
    }

    public void StopOneShotTracked(SoundConfig soundConfig)
    {
        if (soundConfig == null) return;

        string id = soundConfig.name;
        if (_trackedOneShots.TryGetValue(id, out var audioSource))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            _trackedOneShots.Remove(id);
            _poolerOneShot.Set(audioSource);
        }
    }

    public void StopAllOneShotsTracked()
    {
        string[] ids = _trackedOneShots.Keys.ToArray();

        for (int i = 0; i < ids.Length; i++)
        {
            AudioSource audioSource = _trackedOneShots[ids[i]];
            if (audioSource == null) continue;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            _poolerOneShot.Set(audioSource);
        }

        _trackedOneShots.Clear();
    }

    public bool IsTrackedOneShotPlaying(SoundConfig soundConfig)
    {
        if (soundConfig == null) return false;

        string id = soundConfig.name;
        if (_trackedOneShots.TryGetValue(id, out var audioSource))
        {
            return audioSource.isPlaying;
        }

        return false;
    }

    public void PlayLoop(SoundConfig soundConfig, bool shouldRestartIfPlayingAlready = true)
    {
        if (soundConfig == null) return;

        string id = soundConfig.name;

        if (_trackedLoops.TryGetValue(id, out var audioSource))
        {
            // if playing and shouldn't restart, then just apply the effect
            if (audioSource.isPlaying)
            {
                if (shouldRestartIfPlayingAlready)
                {
                    audioSource.Stop();
                }
                else
                {
                    soundConfig.ApplyToSource(audioSource);
                    audioSource.loop = true;
                    audioSource.transform.localPosition = Vector3.zero;
                    return;
                }
            }

            soundConfig.ApplyToSource(audioSource);
            audioSource.loop = true;
            audioSource.transform.localPosition = Vector3.zero;
            audioSource.Play();
            return;
        }

        AudioSource newAudioSource = _poolerLoop.Get(_audioSourcesParent);
        newAudioSource.transform.localPosition = Vector3.zero;

        soundConfig.ApplyToSource(newAudioSource);
        newAudioSource.loop = true;
        newAudioSource.Play();

        _trackedLoops.Add(id, newAudioSource);
    }

    public void StopLoop(SoundConfig soundConfig)
    {
        if (soundConfig == null) return;

        string id = soundConfig.name;
        if (_trackedLoops.TryGetValue(id, out var audioSource))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            _trackedLoops.Remove(id);
            _poolerLoop.Set(audioSource);
        }
    }

    public void StopAllLoops()
    {
        string[] ids = _trackedLoops.Keys.ToArray();

        for (int i = 0; i < ids.Length; i++)
        {
            AudioSource audioSource = _trackedLoops[ids[i]];
            if (audioSource == null) continue;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            _poolerLoop.Set(audioSource);
        }

        _trackedLoops.Clear();
    }

    public bool IsLoopPlaying(SoundConfig soundConfig)
    {
        if (soundConfig == null) return false;

        string id = soundConfig.name;
        if (_trackedLoops.TryGetValue(id, out var audioSource))
        {
            return audioSource.isPlaying;
        }

        return false;
    }

    private IEnumerator ReturnAudioSourceToPoolAfterPlaying(AudioSource audioSource, Guid guid)
    {
        if (audioSource == null) yield break;
        if (audioSource.loop)
        {
            Debug.LogWarning(audioSource + " is looping, can't return after playing");
            yield break;
        }

        yield return new WaitWhile(() => audioSource.isPlaying);

        _poolerOneShot.Set(audioSource);

        if (_activeReturnAudioSourcesCoroutines.ContainsKey(guid))
        {
            _activeReturnAudioSourcesCoroutines.Remove(guid);
        }
    }
}
