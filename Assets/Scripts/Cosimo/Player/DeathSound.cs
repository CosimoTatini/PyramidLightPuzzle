using UnityEngine;

public class DeathSound : MonoBehaviour
{
    [SerializeField] private SoundConfig _soundConfig;
    [SerializeField] private SoundEmitter _emitter;
    public void OnDeath()
    {
        _emitter.PlayOneShot(_soundConfig);
    }
}
