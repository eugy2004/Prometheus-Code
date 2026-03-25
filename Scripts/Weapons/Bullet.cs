using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected Rigidbody rb;
    protected Collider col;

    private float myLifeTime = 30f;
    protected float bulletDamage = 5;
    protected float myPushBackForce;

    private float elapsedTimeSinceBeingShot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        col = GetComponent<Collider>();
    }

    public virtual void OnEnable()
    {
        col.enabled = true;
    }

    public virtual void OnDisable()
    {
        elapsedTimeSinceBeingShot = 0;
    }

    public void DisableBullet()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        elapsedTimeSinceBeingShot += Time.deltaTime;

        if (elapsedTimeSinceBeingShot >= myLifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    protected void PushBack(Rigidbody target)
    {
        Vector3 dir = (target.gameObject.transform.position - transform.position).normalized;
        target.AddForce(dir * myPushBackForce, ForceMode.Impulse);
    }

    public void BulletShoot(
        Vector3 firePoint,
        Vector3 direction,
        float projectileSpeed,
        float lifeTime,
        float damage,
        float size,
        float pushBackForce
    )
    {
        transform.position = firePoint + direction.normalized * 0.6f;

        myLifeTime = lifeTime;
        bulletDamage = damage;
        transform.localScale = Vector3.one * size;
        myPushBackForce = pushBackForce;

        gameObject.SetActive(true);

        rb.linearVelocity = direction.normalized * projectileSpeed;
    }
}