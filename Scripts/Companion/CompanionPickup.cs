using System.Collections;
using UnityEngine;

public class CompanionPickup : MonoBehaviour
{
    [SerializeField] private GameObject companionPrefab;

    private Collider col;

    private void Start()
    {
        StartCoroutine(ActivateColliderCO());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player _))
        {
            CompanionManager manager = other.GetComponentInChildren<CompanionManager>();
            manager.AddCompanion(companionPrefab);
            GameManager.instance.UnlockNextRoom();
        }
    }

    private IEnumerator ActivateColliderCO()
    {
        yield return new WaitForSeconds(6);
        col = GetComponent<Collider>();
        col.enabled = true;
    }
}

