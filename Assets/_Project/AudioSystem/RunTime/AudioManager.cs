
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Data")]
    [SerializeField] private AudioDatabase audioDatabase;

    [Header("Pooled Audio")]
    [SerializeField] private AudioEmitter emitterPrefab;
    [SerializeField] private Transform emitterContainer;
    [Tooltip("Inactive emitters created at initialization. Use 0 to disable prewarming.")]
    [Min(0)] public int prewarmCount = 8;

    private AudioEmitterPool emitterPool;
    private AudioPlaybackHandle currentMusic;
    private AudioID currentMusicId = AudioID.None;

    public AudioPlaybackHandle CurrentMusic => currentMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    
        if (emitterContainer == null)
        {
            emitterContainer = transform;
        }

        emitterPool = GetComponent<AudioEmitterPool>();
        if (emitterPool == null)
        {
            emitterPool = gameObject.AddComponent<AudioEmitterPool>();
        }

        emitterPool.Initialize(emitterPrefab, emitterContainer);
        emitterPool.Prewarm(prewarmCount);
    }

    private void OnEnable()
    {
        if (Instance != this)
        {
            return;
        }

        AudioEvents.OnPlayAudio += HandleAudioRequest;
        AudioEvents.OnPlayControlledAudio += HandleControlledAudioRequest;
        AudioEvents.OnStopAudio += HandleStopRequest;
        AudioEvents.OnSetParameter += HandleParameterRequest;
    }

    private void OnDisable()
    {
        AudioEvents.OnPlayAudio -= HandleAudioRequest;
        AudioEvents.OnPlayControlledAudio -= HandleControlledAudioRequest;
        AudioEvents.OnStopAudio -= HandleStopRequest;
        AudioEvents.OnSetParameter -= HandleParameterRequest;
        if (emitterPool != null)
        {
            emitterPool.StopAll();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // This method plays the specified music track, stopping any currently playing music if necessary.
    public void PlayMusic(AudioID musicId)
    {
        if (musicId == AudioID.None)
        {
            return;
        }

        if (musicId == currentMusicId && currentMusic != null && currentMusic.IsActive)
        {
            return;
        }

        if (audioDatabase == null || !audioDatabase.TryGetDefinition(musicId, out _))
        {
            UnityEngine.Debug.LogWarning(
                $"No valid FMOD event is configured for music AudioID '{musicId}'.",
                this
            );
            return;
        }

        AudioEvents.Stop(currentMusic, allowFadeout: true);
        currentMusic = AudioEvents.PlayControlled(new AudioRequest(musicId));
        currentMusicId = musicId;
    }

    /// <summary>
    /// Stops the scene music owned by this manager, allowing its authored release by default.
    /// </summary>
    public void StopMusic(bool allowFadeout = true)
    {
        AudioEvents.Stop(currentMusic, allowFadeout);
        currentMusic = null;
        currentMusicId = AudioID.None;
    }

    private void HandleAudioRequest(AudioRequest request)
    {
        PlayRequest(request, null);
    }

    private void HandleControlledAudioRequest(AudioRequest request, AudioPlaybackHandle handle)
    {
        PlayRequest(request, handle);
    }

    private void HandleStopRequest(AudioPlaybackHandle handle, bool allowFadeout)
    {
        if (handle.IsActive)
        {
            handle.Emitter.Stop(allowFadeout ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        }
    }

    private void HandleParameterRequest(AudioPlaybackHandle handle, string parameterName, float value)
    {
        if (handle.IsActive)
        {
            handle.Emitter.SetParameter(parameterName, value);
        }
    }

    private void PlayRequest(AudioRequest request, AudioPlaybackHandle handle)
    {
        if (request.ID == AudioID.None)
        {
            UnityEngine.Debug.LogWarning("Audio request has no AudioID.", this);
            return;
        }

        if (audioDatabase == null)
        {
            UnityEngine.Debug.LogError(
                "AudioManager has no AudioDatabase assigned.",
                this
            );
            return;
        }

        if (!audioDatabase.TryGetDefinition(request.ID, out AudioDefinition definition))
        {
            UnityEngine.Debug.LogWarning(
                $"No valid FMOD event is configured for AudioID '{request.ID}'.",
                this
            );
            return;
        }

        switch (request.Mode)
        {
            case AudioPlaybackMode.OneShot:
                PlayOneShot(request, definition);
                break;

            case AudioPlaybackMode.Attached:
            case AudioPlaybackMode.Controlled:
                PlayPooled(request, definition, handle);
                break;

            default:
                UnityEngine.Debug.LogWarning(
                    $"Unsupported audio playback mode '{request.Mode}'.",
                    this
                );
                break;
        }
    }

    private void PlayOneShot(AudioRequest request, AudioDefinition definition)
    {
        EventInstance instance;
        try
        {
            instance = RuntimeManager.CreateInstance(definition.EventReference);
        }
        catch (EventNotFoundException exception)
        {
            UnityEngine.Debug.LogError($"Cannot create one-shot audio: {exception.Message}", this);
            return;
        }

        try
        {
            if (!AudioParameterUtility.CheckResult(
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(request.Position)), "set position", this) ||
                !definition.ApplyTo(instance, request, this))
            {
                return;
            }

            AudioParameterUtility.CheckResult(instance.start(), "start playback", this);
        }
        finally
        {
            // A successfully started one-shot continues playing after release.
            AudioParameterUtility.CheckResult(instance.release(), "release playback", this);
        }
    }

    private void PlayPooled(
        AudioRequest request,
        AudioDefinition definition,
        AudioPlaybackHandle handle)
    {
        if (request.Mode == AudioPlaybackMode.Attached && request.FollowTarget == null)
        {
            UnityEngine.Debug.LogWarning(
                "Attached audio requires a valid Transform."
            );
            return;
        }

        if (emitterPrefab == null)
        {
            UnityEngine.Debug.LogError(
                "AudioManager has no AudioEmitter prefab assigned."
            );
            return;
        }

        AudioEmitter emitter = emitterPool.GetEmitter();
        if (emitter != null)
        {
            emitter.Play(request, definition, handle);
        }
    }
}