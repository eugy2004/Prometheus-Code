using UnityEngine;
using UnityEngine.UI;

public class HeatBarManager : MonoBehaviour
{
    [SerializeField] private Image heatFill;

    void Update()
    {
        if (Player.instance)
            heatFill.fillAmount = Player.instance.GetCurrentHeat() / Player.instance.GetMaxHeat();
    }
}