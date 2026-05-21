using UnityEngine;

public class EnemyHitBox : MonoBehaviour, IHitTarget
{
    [SerializeField] private float hitDamage = 0.0f;

    [SerializeField] private EnemyHealth enemyHealth;

    public void SetEnemyHealth(EnemyHealth newEnemyHealth)
    {
        enemyHealth = newEnemyHealth;
    }

    public void ReceiveHit(float damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        if(enemyHealth != null)
        {
            enemyHealth.ReceiveHit(damage + hitDamage, hitPoint, hitDirection, hitNormal);
        }
    }
}
