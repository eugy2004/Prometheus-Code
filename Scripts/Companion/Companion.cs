using UnityEngine;

public abstract class Companion : MonoBehaviour
{
    protected CompanionSlot slot;

    public virtual void OnAssigned(CompanionSlot newSlot)
    {
        slot = newSlot;
    }

    public virtual void OnRemoved() { }
}

