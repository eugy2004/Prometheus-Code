using UnityEngine;

public class SpriteFollowObject : MonoBehaviour
{
    public Vector3 offset = Vector3.up * 2; // Offset verticale
    public Transform objectToFollow;

    void LateUpdate()
    {
        if (objectToFollow != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                objectToFollow.position + offset,
                Time.deltaTime * 150 // molto più reattivo
            );
        }
    }
}
