using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // Fa sì che il personaggio segua la rotazione della telecamera
        transform.forward = Camera.main.transform.forward;
    }
}
