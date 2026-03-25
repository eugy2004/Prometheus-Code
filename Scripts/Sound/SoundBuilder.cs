using UnityEngine;

public class SoundBuilder
{
    private AudioManager audioManager;

    private SoundData soundData;

    private Vector3 position = Vector3.zero;

    private bool randomPitch;

    public SoundBuilder(AudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public SoundBuilder WithSoundData(SoundData soundData)
    {
        this.soundData = soundData;
        return this;
    }

    public SoundBuilder WithPosition(Vector3 position)
    {
        this.position = position;
        return this;
    }

    public SoundBuilder WithRandomPitch(bool randomPitch)
    {
        this.randomPitch = randomPitch;
        return this;
    }

    public void BuildAndPlay(Transform parent)
    {
        if (!audioManager.CanPlaySound(soundData)) return;

        SoundEmitter emitter = audioManager.Get();
        emitter.Initialize(soundData);
        emitter.transform.position = position;
        emitter.transform.parent = parent;

        if (randomPitch)
        {
            emitter.WithRandomPitch();
        }

        if (soundData.frequentSound)
        {
            audioManager.frequentSoundEmitters.Enqueue(emitter);
        }
        emitter.Play();
    }
}
