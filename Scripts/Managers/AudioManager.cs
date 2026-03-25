using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public SoundLibrary soundLibrary;

    private List<SoundEmitter> activeSoundEmitters = new();

    public Queue<SoundEmitter> frequentSoundEmitters = new();

    IObjectPool<SoundEmitter> soundEmitterPool;

    [SerializeField] private SoundEmitter soundEmitterPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializePool();
    }

    public SoundBuilder CreateSound()
    {
        return new SoundBuilder(this);
    }

    public SoundEmitter Get()
    {
        return soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter emitter)
    {
        soundEmitterPool.Release(emitter);
    }

    public bool CanPlaySound(SoundData data)
    {
        if (!data.frequentSound) return true;

        if (frequentSoundEmitters.Count >= maxSoundInstances && frequentSoundEmitters.TryDequeue(out var soundEmitter))
        {
            try
            {
                soundEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("Failed to stop sound emitter from frequent sound queue.");
                return false;
            }
        }
        return true;
    }

    private void InitializePool()
    {
        soundEmitterPool = new ObjectPool<SoundEmitter>(
            CreateSoundEmitter,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionCheck,
            defaultCapacity,
            maxPoolSize
            );
    }

    private void OnDestroyPoolObject(SoundEmitter emitter)
    {
        return;
    }

    private void OnReturnedToPool(SoundEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(emitter);
    }

    private void OnTakeFromPool(SoundEmitter emitter)
    {
        emitter.gameObject.SetActive(true);
        activeSoundEmitters.Add(emitter);
    }

    private SoundEmitter CreateSoundEmitter()
    {
        var soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }
}
