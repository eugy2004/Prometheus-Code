using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    [SerializeField] private GameObject[] slots;

    private TurretManager turretManager;

    private int slotToModify = 0;

    private void Start()
    {
        turretManager = GetComponent<TurretManager>();
    }

    public void AddCompanion(GameObject companionPrefab)
    {
        GameObject instance = Instantiate(companionPrefab);
        if (instance.TryGetComponent(out TurretCompanion _))
            turretManager.turrets.Add(instance);

        foreach (var slot in slots)
        {
            CompanionSlot companion = slot.GetComponent<CompanionSlot>();
            if (companion.IsFree)
            {
                companion.Assign(instance);
                return;
            }
        }

        if (slots[slotToModify].GetComponentInChildren<Companion>() is TurretCompanion)
        {
            GameObject toRemove = slots[slotToModify].GetComponentInChildren<Companion>().gameObject;
            turretManager.turrets.Remove(toRemove);
        }

        CompanionSlot slotToUse = slots[slotToModify].GetComponent<CompanionSlot>();
        slotToUse.Clear();
        slotToUse.Assign(instance);
        
        if (slotToModify < 2)
        {
            slotToModify++;
        }
        else
        {
            slotToModify = 0;
        }
    }
}

