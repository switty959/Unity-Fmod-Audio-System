using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public AudioID audioID;
    [Header("Local FMOD Parameter Test")]
    public string parameterName;
    public float parameterValue;
    private AudioPlaybackHandle currentHandle;
    private AudioPlaybackHandle previousHandle;
    private readonly List<AudioPlaybackHandle> handles = new List<AudioPlaybackHandle>();

    void Start()
    {
        PlayControlled();
    }

    [ContextMenu("Audio Test/Play Controlled")]
    public void PlayControlled()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        previousHandle = currentHandle;
        currentHandle = AudioEvents.PlayControlled(new AudioRequest(audioID));
        handles.RemoveAll(handle => !handle.IsActive);
        handles.Add(currentHandle);
        Debug.Log($"Controlled audio active: {currentHandle.IsActive}", this);
    }

    [ContextMenu("Audio Test/Stop Immediately")]
    public void StopImmediately()
    {
        if (RequirePlayMode())
        {
            AudioEvents.Stop(currentHandle, allowFadeout: false);
        }
    }

    [ContextMenu("Audio Test/Fade Out")]
    public void FadeOut()
    {
        if (RequirePlayMode())
        {
            AudioEvents.Stop(currentHandle);
        }
    }

    [ContextMenu("Audio Test/Stop Previous Handle")]
    public void StopPreviousHandle()
    {
        if (RequirePlayMode())
        {
            AudioEvents.Stop(previousHandle, allowFadeout: false);
        }
    }

    [ContextMenu("Audio Test/Set Current Parameter")]
    public void SetCurrentParameter()
    {
        if (RequirePlayMode())
        {
            AudioEvents.SetParameter(currentHandle, parameterName, parameterValue);
        }
    }

    [ContextMenu("Audio Test/Set Previous Parameter")]
    public void SetPreviousParameter()
    {
        if (RequirePlayMode())
        {
            AudioEvents.SetParameter(previousHandle, parameterName, parameterValue);
        }
    }

    [ContextMenu("Audio Test/Log Handle State")]
    public void LogHandleState()
    {
        if (RequirePlayMode())
        {
            Debug.Log(
                $"Current active: {currentHandle != null && currentHandle.IsActive}; " +
                $"previous active: {previousHandle != null && previousHandle.IsActive}",
                this
            );
        }
    }

    private bool RequirePlayMode()
    {
        if (Application.isPlaying)
        {
            return true;
        }

        Debug.LogWarning("Audio test actions must be used in Play mode.", this);
        return false;
    }

    private void OnDestroy()
    {
        foreach (AudioPlaybackHandle handle in handles)
        {
            AudioEvents.Stop(handle, allowFadeout: false);
        }
    }
}
