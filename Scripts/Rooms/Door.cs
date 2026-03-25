using UnityEngine;

public class Door : MonoBehaviour
{
    public Collider exitCol;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player _))
        {
            exitCol.enabled = false;
            GameManager.instance.SetUpRoom();
        }
    }
}
