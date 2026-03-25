using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteDepthSorter : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100);
    }
}

