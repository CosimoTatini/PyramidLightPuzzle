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
        Global2DAudioPlayer.Instance.PlayLoop(clipGroup);
    }

    public void PlayOneShotRandom(AudioClipListGroup clipListGroup)
    {
        Global2DAudioPlayer.Instance.PlayOneShotRandom(clipListGroup);
    }

    public void PlayLoop(AudioClipGroup clipGroup)
    {
        Global2DAudioPlayer.Instance.PlayLoop(clipGroup);
    }

    public void PlayLoopRandom(AudioClipListGroup clipListGroup)
    {
        Global2DAudioPlayer.Instance.PlayLoopRandom(clipListGroup);
    }
}
