using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] float lifeTime = 0.25f;
    [SerializeField] float fadeSpeed = 4f;

    private float timer;

    SpriteRenderer sr;
    Color color;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        color = sr.color;
        timer = lifeTime;
    }

    void Update()
    {
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;
        timer -= Time.deltaTime;

        if (timer <= 0)
            Destroy(gameObject);
    }
}