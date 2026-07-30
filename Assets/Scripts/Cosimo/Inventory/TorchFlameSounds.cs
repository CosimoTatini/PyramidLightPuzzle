using UnityEngine;

public class TorchFlameSounds : MonoBehaviour
{
    [SerializeField] private SoundConfig soundConfig;
    private SoundEmitter emitter;

    private void Awake()
    {
        // 🛠️ Garantiamo il recupero del componente al momento del caricamento
        FetchEmitter();
    }

    private void Start()
    {
        // 🎵 'Start' viene eseguito solo DOPO che tutti gli 'Awake' della scena sono stati completati.
        // È il momento più sicuro per avviare l'audio dopo l'instanziazione!
        PlayTorchSound();
    }

    private void OnDisable()
    {
        if (emitter != null && soundConfig != null)
        {
            emitter.StopLoop(soundConfig);
        }
    }

    private void FetchEmitter()
    {
        if (emitter == null)
        {
            emitter = GetComponent<SoundEmitter>();
        }
    }

    private void PlayTorchSound()
    {
        if (emitter != null && soundConfig != null)
        {
            emitter.PlayLoop(soundConfig);
        }
    }
}