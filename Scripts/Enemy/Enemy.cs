using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject parent;

    protected float pushBackResistance;

    private void OnDisable()
    {
        if (Player.instance && Player.instance.CurrentStats.currentHeat >= 0)
            EnemiesManager.instance.DecreaseEnemyCount();
    }

    public virtual void TakeDamage(float amount)
    {

    }

    public virtual void DealDamage(GameObject target)
    {

    }

    protected virtual void Die()
    {
        ResetBehaviourVariables();
        gameObject.SetActive(false);
    }

    public virtual void OnPush(GameObject pusher, float pushForce)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearDamping = pushBackResistance;
        Vector3 pushDir = (transform.position - pusher.transform.position).normalized;
        Vector3 force = pushDir * pushForce;
        force.y = 0;
        rb.AddForce(force * rb.mass, ForceMode.Impulse);
    }

    protected virtual void PushSomething(Collision collision, float pushForce)
    {
        Vector3 pushDir = (collision.gameObject.transform.position - transform.position).normalized;
        Vector3 dir = pushDir * pushForce;
        dir.y = 0;
        collision.rigidbody.AddForce(dir, ForceMode.Impulse);
    }

    public virtual void ResetBehaviourVariables() { }
}
