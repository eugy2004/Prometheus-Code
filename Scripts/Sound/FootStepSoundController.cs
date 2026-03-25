using UnityEngine;

public class FootStepSoundController : MonoBehaviour
{
    [SerializeField] private float footstepCooldown = 0.25f;
    private float lastFootstepTime;

    public void PlayFootstepSFX()
    {
        if (Time.time - lastFootstepTime < footstepCooldown)
            return;

        lastFootstepTime = Time.time;

        AudioManager.instance.CreateSound()
            .WithSoundData(AudioManager.instance.soundLibrary.prometheusSounds.footstepsSound)
            .WithPosition(transform.position)
            .WithRandomPitch(true)
            .BuildAndPlay(transform);
    }
}
