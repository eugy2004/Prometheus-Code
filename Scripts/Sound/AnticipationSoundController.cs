using UnityEngine;

public class AnticipationSoundController : MonoBehaviour
{
    public void PlayAnticipationSFX()
    {
        AudioManager.instance.CreateSound()
            .WithSoundData(AudioManager.instance.soundLibrary.minotaurSounds.anticipationSound)
            .WithPosition(transform.position)
            .WithRandomPitch(true)
            .BuildAndPlay(transform);
    }
}
