using UnityEngine;

public class ShootingSoundController : MonoBehaviour
{
    public void PlayThrowSFX()
    {
        AudioManager.instance.CreateSound()
            .WithSoundData(AudioManager.instance.soundLibrary.prometheusSounds.shootingSound)
            .WithPosition(transform.position)
            .WithRandomPitch(true)
            .BuildAndPlay(transform);
    }
}
