using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class PlayAudio : MonoBehaviour
{
    [SerializeField] private UnityEvent _PlayLoop;

    private void Start()
    {
        _PlayLoop.Invoke();
    }

    public void PlayOneShot(AudioClipGroup clipGroup)
    {
        SFXManager.Instance.PlayLoop(clipGroup);
    }

    public void PlayOneShotRandom(AudioClipListGroup clipListGroup)
    {
        SFXManager.Instance.PlayOneShotRandom(clipListGroup);
    }

    public void PlayLoop(AudioClipGroup clipGroup)
    {
        SFXManager.Instance.PlayLoop(clipGroup);
    }

    public void PlayLoopRandom(AudioClipListGroup clipListGroup)
    {
        SFXManager.Instance.PlayLoopRandom(clipListGroup);
    }
}
