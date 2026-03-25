using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine playingCoroutine;

    public SoundData Data { get; private set; }

    private void Awake()
    {
        audioSource = gameObject.GetOrAddComponent<AudioSource>();
    }

    public void Play()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        audioSource.Play();
        if (!audioSource.loop)
        {
            playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        AudioManager.instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }
        audioSource.Stop();
        AudioManager.instance.ReturnToPool(this);
    }

    public void Initialize(SoundData data)
    {
        Data = data;
        audioSource.clip = data.clip;
        audioSource.outputAudioMixerGroup = data.mixerGroup;
        audioSource.loop = data.loop;
        audioSource.playOnAwake = data.playOnAwake;
        audioSource.priority = data.priority;
        audioSource.volume = data.volume;
        audioSource.panStereo = data.stereoPan;
        audioSource.spatialBlend = data.spatialBlend;
        audioSource.rolloffMode = data.rolloffMode;
        audioSource.minDistance = data.minDistance;
        audioSource.maxDistance = data.maxDistance;
    }

    public void WithRandomPitch()
    {
        audioSource.pitch = Random.Range(Data.minimumPitch, Data.maximumPitch);
    }
}
