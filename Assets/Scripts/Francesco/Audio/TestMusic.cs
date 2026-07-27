using UnityEngine;

public class TestMusic : MonoBehaviour
{
    [SerializeField] private SoundEmitter _soundEmitter;
    [SerializeField] private SoundConfig _backgroundMusic;
    [SerializeField] private SoundConfig _oneShotTest;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _soundEmitter.PlayLoop(_backgroundMusic);
        InvokeRepeating(nameof(PlayOneShot),0f, 0.1f);
    }

    private void PlayOneShot()
    {
        _soundEmitter.PlayOneShot(_oneShotTest);        
    }
}
