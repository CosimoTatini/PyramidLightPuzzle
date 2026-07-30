using UnityEngine;

public class TorchFlameSounds : MonoBehaviour
{
    [SerializeField] private SoundConfig soundConfig;
    private SoundEmitter emitter;

    private void Awake()
    {
        emitter = GetComponent<SoundEmitter>();
    }

    private void OnEnable()
    {
        if(emitter!=null && soundConfig!=null)
        {
            emitter.PlayLoop(soundConfig);
        }
    }

    private void OnDisable()
    {
        if(emitter!=null)
        {
            emitter.StopLoop(soundConfig);
        }
    }


}
