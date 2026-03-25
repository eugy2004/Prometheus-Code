using UnityEngine;

public class CompanionSlot : MonoBehaviour
{
    public Companion CurrentCompanion { get; private set; }

    public bool IsFree => CurrentCompanion == null;

    private void Start()
    {
        CurrentCompanion = null;
    }

    public void Assign(GameObject companion)
    {
        CurrentCompanion = companion.GetComponent<Companion>();
        companion.transform.SetParent(transform);
        companion.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        CurrentCompanion.OnAssigned(this);
    }

    public void Clear()
    {
        Destroy(CurrentCompanion.gameObject);
        CurrentCompanion = null;
    }
}