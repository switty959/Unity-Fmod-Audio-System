
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioEmitter : MonoBehaviour
{
    private EventInstance instance;
    private Transform followTarget;
    private bool hasInstance;
    private AudioEmitterPool pool;
    private AudioPlaybackHandle playbackHandle;
    private bool followsTarget;

    public bool IsActive => hasInstance;

    internal void Initialize(AudioEmitterPool owner)
    {
        pool = owner;
    }

    public void Play(
        AudioRequest request,
        AudioDefinition definition,
        AudioPlaybackHandle handle = null)
    {
        if (hasInstance)
        {
            Debug.LogError("Cannot play an emitter that is already in use.", this);
            return;
        }

        if ((request.Mode == AudioPlaybackMode.Attached && request.FollowTarget == null) ||
            definition == null || definition.EventReference.IsNull)
        {
            Debug.LogWarning(
                "Audio requires a valid FMOD event and attached audio requires a target.",
                this
            );
            Finish();
            return;
        }

        followTarget = request.FollowTarget;
        followsTarget = request.Mode == AudioPlaybackMode.Attached;

        try
        {
            instance = RuntimeManager.CreateInstance(definition.EventReference);
        }
        catch (EventNotFoundException exception)
        {
            Debug.LogError($"Cannot create pooled audio: {exception.Message}", this);
            Finish();
            return;
        }

        hasInstance = true;
        if (!instance.isValid())
        {
            Debug.LogError("FMOD did not create a valid event instance.", this);
            Finish();
            return;
        }

        if (!definition.ApplyTo(instance, request, this))
        {
            Finish();
            return;
        }

        // FMOD's Unity integration updates the instance's
        // 3D attributes as the target moves.
        if (followsTarget)
        {
            RuntimeManager.AttachInstanceToGameObject(
                instance,
                followTarget.gameObject,
                followTarget.GetComponent<Rigidbody>()
            );
        }
        else if (!CheckResult(
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(request.Position)),
            "set position"))
        {
            Finish();
            return;
        }

        if (!CheckResult(instance.start(), "start playback"))
        {
            Finish();
            return;
        }

        playbackHandle = handle;
        if (playbackHandle != null)
        {
            playbackHandle.Emitter = this;
        }
    }

    private void Update()
    {
        if (!hasInstance)
        {
            return;
        }

        if ((followsTarget && followTarget == null) || !RuntimeManager.IsInitialized)
        {
            Stop(STOP_MODE.IMMEDIATE);
            return;
        }

        if (!CheckResult(instance.getPlaybackState(out PLAYBACK_STATE state), "read playback state") ||
            state == PLAYBACK_STATE.STOPPED)
        {
            Finish();
        }
    }

    public void Stop(
        FMOD.Studio.STOP_MODE stopMode =
            FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (!hasInstance || !RuntimeManager.IsInitialized || stopMode == STOP_MODE.IMMEDIATE)
        {
            Finish();
            return;
        }

        if (!CheckResult(instance.stop(stopMode), "stop playback"))
        {
            Finish();
        }
    }

    internal void SetParameter(string parameterName, float value)
    {
        if (!hasInstance || !RuntimeManager.IsInitialized || !instance.isValid())
        {
            Debug.LogWarning($"Cannot set audio parameter '{parameterName}' without a valid playback.", this);
            return;
        }

        AudioParameterUtility.SetParameter(instance, parameterName, value, this);
    }

    private void Finish()
    {
        if (hasInstance)
        {
            // FMOD may already have shut down when Unity destroys scene objects.
            if (RuntimeManager.IsInitialized)
            {
                RuntimeManager.DetachInstanceFromGameObject(instance);
                if (instance.isValid())
                {
                    CheckResult(instance.stop(STOP_MODE.IMMEDIATE), "stop playback");
                    CheckResult(instance.release(), "release playback");
                }
            }

            instance.clearHandle();
        }

        hasInstance = false;
        followTarget = null;
        followsTarget = false;
        if (playbackHandle != null)
        {
            playbackHandle.Emitter = null;
            playbackHandle = null;
        }

        if (pool != null)
        {
            pool.ReturnEmitter(this);
        }
    }

    private bool CheckResult(FMOD.RESULT result, string operation)
    {
        if (result == FMOD.RESULT.OK)
        {
            return true;
        }

        Debug.LogError($"AudioEmitter could not {operation}: {result}.", this);
        return false;
    }

    private void OnDisable()
    {
        Stop(STOP_MODE.IMMEDIATE);
    }

    private void OnDestroy()
    {
        Stop(STOP_MODE.IMMEDIATE);
        if (pool != null)
        {
            pool.RemoveEmitter(this);
        }
    }
}