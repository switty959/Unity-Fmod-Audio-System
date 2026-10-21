
using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(
    fileName = "AudioDatabase",
    menuName = "Audio/Audio Database"
)]
public class AudioDatabase : ScriptableObject
{
    [SerializeField]
    private List<AudioDefinition> entries = new List<AudioDefinition>();

    private void OnValidate()
    {
        ValidateEntries();
    }

    [ContextMenu("Validate Audio Database")]
    private void ValidateDatabase()
    {
        if (ValidateEntries())
        {
            Debug.Log($"AudioDatabase '{name}' validation passed ({entries.Count} entries).", this);
        }
    }

    private bool ValidateEntries()
    {
        bool isValid = true;
        var firstEntryByID = new Dictionary<AudioID, int>();

        for (int i = 0; i < entries.Count; i++)
        {
            AudioDefinition entry = entries[i];
            string entryLabel = $"AudioDatabase '{name}', entry {i + 1}";

            if (entry == null)
            {
                Debug.LogWarning($"{entryLabel} is null. Remove it or configure an audio entry.", this);
                isValid = false;
                continue;
            }

            if (entry.ID == AudioID.None || !Enum.IsDefined(typeof(AudioID), entry.ID))
            {
                Debug.LogWarning($"{entryLabel} has an invalid AudioID '{entry.ID}'. Select a valid sound ID.", this);
                isValid = false;
            }

            if (firstEntryByID.TryGetValue(entry.ID, out int firstIndex))
            {
                Debug.LogWarning(
                    $"{entryLabel} duplicates AudioID '{entry.ID}' from entry {firstIndex + 1}. Each ID must be unique.",
                    this
                );
                isValid = false;
            }
            else
            {
                firstEntryByID.Add(entry.ID, i);
            }

            if (entry.EventReference.IsNull)
            {
                Debug.LogWarning($"{entryLabel} ('{entry.ID}') has no FMOD event assigned.", this);
                isValid = false;
            }

            if (!entry.ValidateSettings(this, entryLabel))
            {
                isValid = false;
            }
        }

        return isValid;
    }

    public bool TryGetEvent(AudioID id, out EventReference eventReference)
    {
        if (TryGetDefinition(id, out AudioDefinition definition))
        {
            eventReference = definition.EventReference;
            return true;
        }

        eventReference = default;
        return false;
    }

    public bool TryGetDefinition(AudioID id, out AudioDefinition definition)
    {
        foreach (AudioDefinition entry in entries)
        {
            if (entry != null && entry.ID == id)
            {
                definition = entry;
                return !entry.EventReference.IsNull;
            }
        }

        definition = null;
        return false;
    }
}
