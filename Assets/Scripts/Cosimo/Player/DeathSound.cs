using UnityEngine;

public class DeathSound : MonoBehaviour
{
    [SerializeField] private SoundConfig _soundConfig;
     private SoundEmitter _emitter;

    private void Awake()
    {
        _emitter = GetComponentInParent<SoundEmitter>();
    }

    public void OnDeath()
    {
        _emitter.PlayOneShot(_soundConfig);
    }
}
