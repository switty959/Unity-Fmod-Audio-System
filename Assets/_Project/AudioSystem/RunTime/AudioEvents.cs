using System;
using UnityEngine;

public sealed class AudioPlaybackHandle
{
    internal AudioEmitter Emitter { get; set; }

    public bool IsActive => Emitter != null && Emitter.IsActive;

    internal AudioPlaybackHandle()
    {
    }
}

public static class AudioEvents
{
    public static event Action<AudioRequest> OnPlayAudio;
    public static event Action<AudioRequest, AudioPlaybackHandle> OnPlayControlledAudio;
    public static event Action<AudioPlaybackHandle, bool> OnStopAudio;
    public static event Action<AudioPlaybackHandle, string, float> OnSetParameter;

    public static void Play(AudioRequest request)
    {
        OnPlayAudio?.Invoke(request);
    }

    /// <summary>
    /// Plays through a pooled emitter. Attached requests follow their target;
    /// other requests stay at their position. Failed requests return an inactive handle.
    /// </summary>
    public static AudioPlaybackHandle PlayControlled(AudioRequest request)
    {
        var handle = new AudioPlaybackHandle();
        if (OnPlayControlledAudio == null)
        {
            Debug.LogWarning("Cannot play controlled audio without an enabled AudioManager.");
            return handle;
        }

        if (request.Mode == AudioPlaybackMode.OneShot)
        {
            request.Mode = AudioPlaybackMode.Controlled;
        }

        OnPlayControlledAudio.Invoke(request, handle);
        return handle;
    }

    /// <summary>
    /// Stops only this playback. Fade-out uses the release behavior authored in FMOD.
    /// </summary>
    public static void Stop(AudioPlaybackHandle handle, bool allowFadeout = true)
    {
        // Finished handles are intentionally harmless, even after their emitter is reused.
        if (handle == null || !handle.IsActive)
        {
            return;
        }

        if (OnStopAudio == null)
        {
            Debug.LogWarning("Cannot stop controlled audio without an enabled AudioManager.");
            return;
        }

        OnStopAudio.Invoke(handle, allowFadeout);
    }

    /// <summary>
    /// Updates a local, game-controlled numeric parameter on this playback.
    /// FMOD applies the parameter's range and authored seek speed.
    /// </summary>
    public static void SetParameter(AudioPlaybackHandle handle, string parameterName, float value)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || float.IsNaN(value) || float.IsInfinity(value))
        {
            Debug.LogWarning("Audio parameter updates require a name and a finite numeric value.");
            return;
        }

        if (handle == null || !handle.IsActive)
        {
            Debug.LogWarning($"Cannot set audio parameter '{parameterName}' on an inactive playback.");
            return;
        }

        if (OnSetParameter == null)
        {
            Debug.LogWarning("Cannot set an audio parameter without an enabled AudioManager.");
            return;
        }

        OnSetParameter.Invoke(handle, parameterName, value);
    }
}