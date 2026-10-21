using System;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[Serializable]
public class AudioDefinition
{
    [Serializable]
    public class Parameter
    {
        public string Name;
        public float Value;
    }

    public AudioID ID;
    public EventReference EventReference;
    [Min(0f)] public float DefaultVolume = 1f;
    public List<Parameter> DefaultParameters = new List<Parameter>();

    internal bool ValidateSettings(UnityEngine.Object context, string label)
    {
        bool valid = true;
        if (float.IsNaN(DefaultVolume) || float.IsInfinity(DefaultVolume) || DefaultVolume < 0f)
        {
            Debug.LogWarning($"{label} requires a finite, non-negative default volume.", context);
            valid = false;
        }

        if (DefaultParameters == null)
        {
            Debug.LogWarning($"{label} has a null default parameter list.", context);
            return false;
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Parameter parameter in DefaultParameters)
        {
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.Name) ||
                float.IsNaN(parameter.Value) || float.IsInfinity(parameter.Value))
            {
                Debug.LogWarning($"{label} has a default parameter without a name or finite value.", context);
                valid = false;
            }
            else if (!names.Add(parameter.Name))
            {
                Debug.LogWarning($"{label} has duplicate default parameter '{parameter.Name}'.", context);
                valid = false;
            }
        }

        return valid;
    }

    /// <summary>
    /// Applies defaults before start. Request volume multiplies the sound's default volume.
    /// Initial parameters bypass seek speed so playback begins at the configured values.
    /// </summary>
    internal bool ApplyTo(EventInstance instance, AudioRequest request, UnityEngine.Object context)
    {
        if (!ValidateSettings(context, $"Audio definition '{ID}'"))
        {
            return false;
        }

        float volume = DefaultVolume * request.Volume;
        if (float.IsNaN(volume) || float.IsInfinity(volume) || volume < 0f)
        {
            Debug.LogWarning($"Audio definition '{ID}' produced an invalid playback volume.", context);
            return false;
        }

        if (!AudioParameterUtility.CheckResult(instance.setVolume(volume), "set volume", context) ||
            !AudioParameterUtility.CheckResult(instance.setPitch(request.Pitch), "set pitch", context))
        {
            return false;
        }

        foreach (Parameter parameter in DefaultParameters)
        {
            if (!AudioParameterUtility.SetParameter(instance, parameter.Name, parameter.Value, context, true))
            {
                return false;
            }
        }

        return true;
    }
}

internal static class AudioParameterUtility
{
    internal static bool SetParameter(
        EventInstance instance, string name, float value, UnityEngine.Object context,
        bool ignoreSeekSpeed = false)
    {
        if (!CheckResult(instance.getDescription(out EventDescription description), "get event description", context) ||
            !CheckResult(description.getParameterDescriptionByName(name, out PARAMETER_DESCRIPTION parameter),
                $"find parameter '{name}'", context))
        {
            return false;
        }

        if ((parameter.flags & (PARAMETER_FLAGS.GLOBAL | PARAMETER_FLAGS.READONLY |
            PARAMETER_FLAGS.AUTOMATIC)) != 0)
        {
            Debug.LogWarning(
                $"Audio parameter '{name}' must be local and game-controlled, not global, automatic, or read-only.",
                context);
            return false;
        }

        return CheckResult(instance.setParameterByID(parameter.id, value, ignoreSeekSpeed),
            $"set parameter '{name}'", context);
    }

    internal static bool CheckResult(FMOD.RESULT result, string operation, UnityEngine.Object context)
    {
        if (result == FMOD.RESULT.OK)
        {
            return true;
        }

        Debug.LogError($"Audio could not {operation}: {result}.", context);
        return false;
    }
}
