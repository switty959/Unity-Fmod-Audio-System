using System.Collections.Generic;
using UnityEngine;

public class AudioEmitterPool : MonoBehaviour
{
    private readonly Queue<AudioEmitter> availableEmitters = new Queue<AudioEmitter>();
    private readonly HashSet<AudioEmitter> activeEmitters = new HashSet<AudioEmitter>();
    private readonly HashSet<AudioEmitter> ownedEmitters = new HashSet<AudioEmitter>();
    private AudioEmitter emitterPrefab;
    private Transform emitterContainer;

    public int ActiveCount => activeEmitters.Count;
    public int AvailableCount => availableEmitters.Count;

    public void Initialize(AudioEmitter prefab, Transform container)
    {
        emitterPrefab = prefab;
        emitterContainer = container != null ? container : transform;
    }

    /// <summary>
    /// Ensures this many emitters exist, without limiting later pool growth.
    /// </summary>
    public void Prewarm(int count)
    {
        if (count < 0)
        {
            Debug.LogWarning("AudioEmitterPool prewarm count cannot be negative.", this);
            return;
        }

        if (count <= ownedEmitters.Count || !ValidateConfiguration())
        {
            return;
        }

        while (ownedEmitters.Count < count)
        {
            AudioEmitter emitter = CreateEmitter();
            emitter.gameObject.SetActive(false);
            availableEmitters.Enqueue(emitter);
        }
    }

    private AudioEmitter CreateEmitter()
    {
        AudioEmitter emitter = Instantiate(emitterPrefab, emitterContainer);
        emitter.Initialize(this);
        ownedEmitters.Add(emitter);
        return emitter;
    }

    private bool ValidateConfiguration()
    {
        if (!isActiveAndEnabled || emitterPrefab == null)
        {
            Debug.LogError("AudioEmitterPool requires an enabled pool and an emitter prefab.", this);
            return false;
        }

        if (emitterContainer == null || !emitterContainer.gameObject.activeInHierarchy ||
            !emitterPrefab.enabled)
        {
            Debug.LogError("AudioEmitterPool requires an active container and an enabled emitter prefab.", this);
            return false;
        }

        return true;
    }

    public AudioEmitter GetEmitter()
    {
        if (!ValidateConfiguration())
        {
            return null;
        }

        AudioEmitter emitter = null;
        while (availableEmitters.Count > 0 && emitter == null)
        {
            emitter = availableEmitters.Dequeue();
        }

        if (emitter == null)
        {
            emitter = CreateEmitter();
        }

        activeEmitters.Add(emitter);
        emitter.enabled = true;
        emitter.gameObject.SetActive(true);
        return emitter;
    }

    internal void ReturnEmitter(AudioEmitter emitter)
    {
        if (!activeEmitters.Remove(emitter))
        {
            return;
        }

        emitter.gameObject.SetActive(false);
        availableEmitters.Enqueue(emitter);
    }

    internal void RemoveEmitter(AudioEmitter emitter)
    {
        activeEmitters.Remove(emitter);
        ownedEmitters.Remove(emitter);

        int count = availableEmitters.Count;
        for (int i = 0; i < count; i++)
        {
            AudioEmitter available = availableEmitters.Dequeue();
            if (available != null && available != emitter)
            {
                availableEmitters.Enqueue(available);
            }
        }
    }

    public void StopAll()
    {
        // Stopping returns emitters to the pool and changes the active collection.
        foreach (AudioEmitter emitter in new List<AudioEmitter>(activeEmitters))
        {
            if (emitter != null)
            {
                emitter.Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    private void OnDisable()
    {
        StopAll();
    }

    private void OnDestroy()
    {
        foreach (AudioEmitter emitter in new List<AudioEmitter>(ownedEmitters))
        {
            if (emitter != null)
            {
                Destroy(emitter.gameObject);
            }
        }

        activeEmitters.Clear();
        availableEmitters.Clear();
        ownedEmitters.Clear();
    }
}
