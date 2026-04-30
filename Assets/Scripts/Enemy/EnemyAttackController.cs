using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    [SerializeField] private EnemyTargetDetector targetDetector;
    [SerializeField] private EnemyChaseController chaseController;

    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private bool attackOnlyWhenStopped = true;

    [SerializeField] private bool hasPlayerHealthTarget = false;
    [SerializeField] private bool canAttack = false;

    private PlayerHealth cachedPlayerHealth;
    private Transform cachedTargetTransform;
    private float lastAttackTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        UpdateTargetHealthCache();
        UpdateAttackState();

        if(canAttack == true)
        {
            Attack();
        }
    }

    void UpdateTargetHealthCache()
    {
        Transform targetTransform = targetDetector.TargetTransform;

        if(targetTransform == cachedTargetTransform)
        {
            return;
        }

        cachedTargetTransform = targetTransform;
        cachedPlayerHealth = cachedTargetTransform.GetComponent<PlayerHealth>();

        hasPlayerHealthTarget = cachedPlayerHealth != null;
    }

    void UpdateAttackState()
    {
        float currentTime = Time.time;
        float elapsedTimeSinceLastAttack = currentTime - lastAttackTime;
        float remainingTime = attackInterval - elapsedTimeSinceLastAttack;

        bool hasTarget = cachedPlayerHealth != null;
        bool targetAlive = hasTarget == true && cachedPlayerHealth.IsDead == false;
        bool cooldownReady = elapsedTimeSinceLastAttack >= attackInterval;

        bool stateAllowAttack = true;

        if(attackOnlyWhenStopped == true)
        {
            stateAllowAttack = chaseController != null && chaseController.CurrentState == EnemyAIState.Stopped;
        }

        canAttack = hasTarget == true && targetAlive == true && cooldownReady == true && stateAllowAttack == true;
    }

    void Attack()
    {
        if(cachedPlayerHealth == null)
        {
            return;
        }

        lastAttackTime = Time.time;
        cachedPlayerHealth.ReceiveDamage(attackDamage, transform.position);
    }
}
