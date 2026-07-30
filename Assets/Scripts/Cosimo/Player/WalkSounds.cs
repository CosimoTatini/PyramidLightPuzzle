using UnityEngine;

public class WalkSounds : MonoBehaviour
{
    [SerializeField] private SoundConfig _soundConfig;

   [SerializeField] private SoundEmitter _soundEmitter;

    // Call this when entering the walking state
    public void StartWalking()
    {
        if (_soundEmitter != null && _soundConfig != null)
        {
            _soundEmitter.PlayLoop(_soundConfig);
        }
    }

    // Call this when exiting to Idle or stopping
    public void StopWalking()
    {
        if (_soundEmitter != null)
        {
            _soundEmitter.StopLoop(_soundConfig);
        }
    }
}