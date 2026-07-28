using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSoundConfig", menuName = "Audio/Sound Config")]
public class SoundConfig : ScriptableObject
{
    [Header("Audio Clip")]
    [Tooltip("The audio file to be played.")]
    [SerializeField] private AudioClip _clip;
    public AudioClip Clip => _clip;

    [Tooltip("Whether the played clip is random, if false Clip will always be played, if true a random clip will be played among Clip and ClipsAlternatives")]
    [SerializeField] private bool _randomize = false;
    public bool Randomize => _randomize;
    [Tooltip("Clips pool (together with Clip) from which a random clip is chosen")]
    [SerializeField] private AudioClip[] _clipsAlternatives;
    public AudioClip[] ClipsAlternatives => _clipsAlternatives;

    [Header("Mixer")]
    [Tooltip("The AudioMixerGroup that this sound routes through. Used for volume control, ducking, and applying effects.")]
    [SerializeField] private AudioMixerGroup _mixerGroup;
    public AudioMixerGroup MixerGroup => _mixerGroup;

    [Header("Playback")]
    [Tooltip("Priority of the sound (0 = highest priority, 256 = lowest). If too many sounds play simultaneously, lower priority sounds are cut off first to save performance.")]
    [SerializeField][Range(0, 256)] private int _priority = 128;

    [Tooltip("Whether the audio clip should loop indefinitely. Note: The SoundEmitter overrides this; use PlayLoop() for looping behavior instead.")]
    [SerializeField] private bool _loop = false;

    [Tooltip("Base volume of the sound (0 = silent, 1 = full volume).")]
    [SerializeField][Range(0f, 1f)] private float _volume = 1f;

    [Tooltip("Random variation added to the volume (± this value). Creates subtle differences between repeated sounds (e.g., footsteps).")]
    [SerializeField] private float _volumeVariance = 0.05f;

    [Tooltip("Base pitch of the sound (1.0 = original speed). Values < 1.0 slow it down, > 1.0 speed it up.")]
    [SerializeField][Range(-3f, 3f)] private float _pitch = 1f;

    [Tooltip("Random variation added to the pitch (± this value). Essential for making sounds feel organic (e.g., every footstep sounds slightly different).")]
    [SerializeField] private float _pitchVariance = 0.05f;

    [Header("Spatial (3D)")]
    [Tooltip("How much 3D spatialization to apply. 0 = fully 2D (no distance attenuation or panning), 1 = fully 3D (positional audio).")]
    [SerializeField][Range(0f, 1f)] private float _spatialBlend = 1f;
    [Tooltip("How the audio fades with distance")]
    [SerializeField] private AnimationCurve _volumeRolloffCurve;

    [Tooltip("How much the pitch changes based on the velocity of the source relative to the listener. 0 = Doppler effect disabled.")]
    [SerializeField][Range(0f, 5f)] private float _dopplerLevel = 1f;

    [Tooltip("The distance (in world units) at which the sound is at full volume. Within this radius, volume remains at maximum.")]
    [SerializeField] private float _minDistance = 1f;

    [Tooltip("The distance (in world units) where the sound becomes completely inaudible. Volume fades to zero at this point.")]
    [SerializeField] private float _maxDistance = 10f;

    // [Tooltip("How the volume decreases with distance. Logarithmic (most natural), Linear (constant drop), or Custom (uses a curve defined in the Audio Source).")]
    // [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Panning & Spread")]
    [Tooltip("Panning in 2D stereo space (-1 = full left, 0 = centered, 1 = full right). Overridden by 3D positional panning when Spatial Blend > 0.")]
    [SerializeField][Range(-1f, 1f)] private float _panStereo = 0f;

    [Tooltip("Perceived width of the sound in 3D space (0 = point source, 360 = fully surround / omnidirectional).")]
    [SerializeField][Range(0f, 360f)] private float _spread = 0f;

    [Header("Effects & Bypass")]
    [Tooltip("How much Reverb Zones affect this sound (0 = completely dry/no reverb, 1 = fully wet/reverberated).")]
    [SerializeField][Range(0f, 1.1f)] private float _reverbZoneMix = 1f;

    [Tooltip("Bypasses all effects applied to this sound's AudioMixerGroup (and parent groups). Useful for UI sounds that shouldn't be processed.")]
    [SerializeField] private bool _bypassEffects = false;

    [Tooltip("Bypasses all effects applied to the AudioListener (such as global reverb, echo, or EQ).")]
    [SerializeField] private bool _bypassListenerEffects = false;

    [Tooltip("Bypasses the effect of Reverb Zones in the scene for this sound. Useful for sounds that should always sound 'dry'.")]
    [SerializeField] private bool _bypassReverbZones = false;

    [Header("Listener Interaction")]
    [Tooltip("If enabled, this sound ignores the global Listener volume slider. Useful for UI or critical sound effects.")]
    [SerializeField] private bool _ignoreListenerVolume = false;

    [Tooltip("If enabled, this sound continues playing even when the game is paused via AudioListener.pause. Useful for menus or UI overlays.")]
    [SerializeField] private bool _ignoreListenerPause = false;

    [Header("Advanced")]
    [Tooltip("How the AudioSource updates its velocity for Doppler effect calculations. Auto (uses Transform), Fixed (fixed timestep), or Dynamic (continuous).")]
    [SerializeField] private AudioVelocityUpdateMode _velocityUpdateMode = AudioVelocityUpdateMode.Auto;

    // --- Randomization Helpers ---
    public float GetRandomVolume()
    {
        return Mathf.Clamp01(_volume + Random.Range(-_volumeVariance, _volumeVariance));
    }

    public float GetRandomPitch()
    {
        return Mathf.Clamp(_pitch + Random.Range(-_pitchVariance, _pitchVariance), 0.1f, 3f);
    }

    // --- Apply ALL settings to an AudioSource ---
    public void ApplyToSource(AudioSource source, bool randomizePitch = true, bool randomizeVolume = true, bool dontOverrideClip = false)
    {
        // Core
        if (!dontOverrideClip)
        {
            if (_randomize)
            {
                int randomIndex = Random.Range(0, _clipsAlternatives.Length + 1);
                // playing _clip
                if (randomIndex == 0)
                {
                    source.clip = _clip;
                }
                else
                {
                    source.clip = _clipsAlternatives[randomIndex - 1];
                }
            }
            else
            {
                source.clip = _clip;
            }
        }
        source.outputAudioMixerGroup = _mixerGroup;
        source.priority = _priority;
        source.loop = _loop;

        // Volume & Pitch (with optional randomization)
        source.volume = randomizeVolume ? GetRandomVolume() : _volume;
        source.pitch = randomizePitch ? GetRandomPitch() : _pitch;

        // Spatial (3D)
        source.spatialBlend = _spatialBlend;
        source.dopplerLevel = _dopplerLevel;
        source.minDistance = _minDistance;
        source.maxDistance = _maxDistance;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _volumeRolloffCurve);
        source.rolloffMode = AudioRolloffMode.Custom;

        // Panning & Spread
        source.panStereo = _panStereo;
        source.spread = _spread;

        // Effects
        source.reverbZoneMix = _reverbZoneMix;
        source.bypassEffects = _bypassEffects;
        source.bypassListenerEffects = _bypassListenerEffects;
        source.bypassReverbZones = _bypassReverbZones;

        // Listener Interaction
        source.ignoreListenerVolume = _ignoreListenerVolume;
        source.ignoreListenerPause = _ignoreListenerPause;

        // Advanced
        source.velocityUpdateMode = _velocityUpdateMode;
    }

#if UNITY_EDITOR
    void Reset()
    {
        SetupDefaultVolumeCurve();
    }

[ContextMenu("Setup Default Curve")]
public void SetupDefaultVolumeCurve()
{
    // --- Safety Guards ---
    if (_maxDistance <= 0) _maxDistance = 10f;
    if (_minDistance < 0) _minDistance = 1f;
    if (_minDistance >= _maxDistance) _minDistance = _maxDistance;

    float normalizedMin = _minDistance / _maxDistance;
    _volumeRolloffCurve = new AnimationCurve();

    // --- Settings for the true logarithmic shape ---
    // Base of the logarithm. Higher base -> steeper drop right after minDistance,
    // flatter fade towards maxDistance. 10 is a good default.
    float logBase = 20f;   // You can expose this as a serialized field if desired

    // Number of points to bake; 30 is enough for a smooth curve.
    int numPoints = 30;

    for (int i = 0; i <= numPoints; i++)
    {
        float t = (float)i / numPoints;
        float x = Mathf.Lerp(0f, 1f, t);
        float volume;

        // Flat region: full volume up to minDistance
        if (x <= normalizedMin)
        {
            volume = 1f;
        }
        else
        {
            // Progress from 0 at minDistance to 1 at maxDistance
            float progress = (x - normalizedMin) / (1f - normalizedMin);

            // --- TRUE LOGARITHMIC FORMULA ---
            // volume = 1 - log_base( 1 + progress * (base - 1) )
            // This gives: at progress=0 -> 1, at progress=1 -> 0,
            // with a true logarithmic drop in between.
            float logVal = Mathf.Log(1f + progress * (logBase - 1f), logBase);
            volume = 1f - logVal;
        }

        _volumeRolloffCurve.AddKey(x, volume);
    }

    // Force linear tangents between all keyframes to prevent any
    // "ease‑in‑ease‑out" interpolation artifacts.
#if UNITY_EDITOR
    for (int i = 0; i < _volumeRolloffCurve.keys.Length; i++)
    {
        UnityEditor.AnimationUtility.SetKeyLeftTangentMode(_volumeRolloffCurve, i, UnityEditor.AnimationUtility.TangentMode.Linear);
        UnityEditor.AnimationUtility.SetKeyRightTangentMode(_volumeRolloffCurve, i, UnityEditor.AnimationUtility.TangentMode.Linear);
    }
#endif
}
    void OnValidate()
    {
        SetupDefaultVolumeCurve();
    }
#endif
}