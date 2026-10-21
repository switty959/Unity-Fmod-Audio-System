
using UnityEngine;

public enum AudioPlaybackMode
{
    OneShot,
    Attached,
    Controlled
}

public struct AudioRequest
{
    public AudioID ID;
    public AudioPlaybackMode Mode;

    public Vector3 Position;
    public Transform FollowTarget;

    public float Volume;
    public float Pitch;

    public AudioRequest(
        AudioID id,
        Vector3 position,
        float volume = 1f,
        float pitch = 1f)
    {
        ID = id;
        Mode = AudioPlaybackMode.OneShot;
        Position = position;
        FollowTarget = null;
        Volume = volume;
        Pitch = pitch;
    }
    public AudioRequest(
        AudioID id,
        float volume = 1f,
        float pitch = 1f)
    {
        ID = id;
        Mode = AudioPlaybackMode.OneShot;
        Position = Vector3.zero;
        FollowTarget = null;
        Volume = volume;
        Pitch = pitch;
    }


    public AudioRequest(
        AudioID id,
        Transform followTarget,
        float volume = 1f,
        float pitch = 1f)
    {
        ID = id;
        Mode = AudioPlaybackMode.Attached;
        Position = followTarget != null
            ? followTarget.position
            : Vector3.zero;

        FollowTarget = followTarget;
        Volume = volume;
        Pitch = pitch;
    }
}