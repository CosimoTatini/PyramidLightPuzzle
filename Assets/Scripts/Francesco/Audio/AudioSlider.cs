using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [Header("Vars")]
    [SerializeField] private Slider _slider;

    [SerializeField, Range(0.0001f, 1f)] private float _minAudioValue = 0.0001f;
    [SerializeField, Range(0.0001f, 1f)] private float _maxAudioValue;

    [SerializeField] private AudioMixerGroup _mixerGroup;
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Events")]
    [SerializeField] private UnityEvent OnMute;
    [SerializeField] private UnityEvent OnUnmute;

    private const string VOLUME = "Volume";

    private float _value;
    private bool _isMuted = false;
    private float _beforeMuteValue;

    private void Awake()
    {
        LoadValue();
        SetUpSlider();
        UpdateValue(_value);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_minAudioValue > _maxAudioValue)
        {
            _minAudioValue = _maxAudioValue;
        }
    }
#endif

    private void Start()
    {
        // Update the value in Start,
        // if done on Awake AudioMixer won't be ready and setting the Volume won't work
        UpdateValue(_value);
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(UpdateValue);
    }

    private void LoadValue()
    {
        if (!PlayerPrefs.HasKey(_mixerGroup.name))
        {
            if (_audioMixer.GetFloat(_mixerGroup.name + VOLUME, out float dB))
            {
                float linearValue = Mathf.Pow(10, dB / 20f);
                _value = Mathf.Clamp(linearValue, _minAudioValue, _maxAudioValue);

                PlayerPrefs.SetFloat(_mixerGroup.name, _value);
                PlayerPrefs.Save();
            }
        }
        else
        {
            _value = PlayerPrefs.GetFloat(_mixerGroup.name); // This is correct (linear)
        }
    }

    private void SetUpSlider()
    {
        _slider.minValue = _minAudioValue;
        _slider.maxValue = _maxAudioValue;
        _slider.value = _value;
    }

    private void UpdateValue(float value)
    {
        float normalized = Mathf.Max(value, _minAudioValue);

        float db = Mathf.Log10(normalized) * 20f;

        bool result = _audioMixer.SetFloat(_mixerGroup.name + VOLUME, db);

        if (!result)
        {
            Debug.LogWarning("Property: " + _mixerGroup.name + VOLUME + " not found");
            return;
        }

        _value = value;
        SaveValue();

        if (value == _minAudioValue)
        {
            _isMuted = true;
            OnMute.Invoke();
        }
        else if (_isMuted)
        {
            _isMuted = false;
            OnUnmute.Invoke();
        }
    }

    public void SetValue(float value)
    {
        UpdateValue(value);
        _slider.value = value;
    }

    public void Mute()
    {
        _beforeMuteValue = _value;
        SetValue(_minAudioValue);
    }

    public void Unmute()
    {
        SetValue(_beforeMuteValue);
    }

    private void SaveValue()
    {
        PlayerPrefs.SetFloat(_mixerGroup.name, _value);
        PlayerPrefs.Save();
    }
}
