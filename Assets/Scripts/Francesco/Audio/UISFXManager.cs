using DesignPatterns.Generics;
using System;
using UnityEngine;

public class UISFXManager : Singleton<UISFXManager>
{
    public AudioClipListGroup OnEnterSoundList;
    public float DelayEnterSound = 0.5f;
    public bool IsOnEnterSoundPlaying = false;

    public void PlayOnEnterSound()
    {
        if (IsOnEnterSoundPlaying) return;

        IsOnEnterSoundPlaying = true;
        SFXManager.Instance.PlayOneShotRandom(OnEnterSoundList);
        
        Invoke(nameof(ResetOnEnterSoundPlaying), DelayEnterSound);
    }

    private void ResetOnEnterSoundPlaying()
    {
        IsOnEnterSoundPlaying = false;
    }
}
