using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IHitTarget
{
    [SerializeField] private float maxHealth = 50.0f;
    [SerializeField] private float currentHealth = 0.0f;

    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 1.5f;

    [SerializeField] private bool isDead = false;

    [SerializeField] private Collider collider; 
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyChaseController chaseController;
    [SerializeField] private EnemyAttackController attackController;
    [SerializeField] private EnemyRagdoll ragdoll;

    [SerializeField] private Renderer renderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitDuration = 0.2f;

    private Material[] materials;
    private Color[] originalColors;
    Coroutine hitRoutine = null;

    private void Awake()
    {
        currentHealth = maxHealth;
        materials = renderer.materials;

        originalColors = new Color[materials.Length];
        for(int i=0; i<materials.Length; ++i)
        {
            originalColors[i] = materials[i].color;
        }
    }

    public void ReceiveHit(float damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        if(isDead == true)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0.0f, currentHealth);

        ApplyHitColor();

        if (currentHealth == 0.0f)
        {
            Die();
        }
    }

    void Die()
    {
        if(isDead == true)
        {
            return;
        }

        isDead = true;

        // AI 기능 끄기.
        StopMovementAndAttack();
        DisableCollider();

        ragdoll.EnableRagdoll();

        if (destroyOnDeath == true)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void StopMovementAndAttack()
    {
        if(navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
        }

        if(chaseController != null)
        {
            chaseController.enabled = false;
        }

        if(attackController != null)
        {
            attackController.enabled = false;
        }
    }

    void DisableCollider()
    {
        if(collider != null)
        {
            collider.enabled = false;
        }
    }

    void ApplyHitColor()
    {
        if(hitRoutine != null)
        {
            StopCoroutine(HitColorRoutine());
        }

        hitRoutine = StartCoroutine(HitColorRoutine());
    }

    IEnumerator HitColorRoutine()
    {
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i].color = hitColor;
        }

        yield return new WaitForSeconds(hitDuration);

        RestoreColor();

        hitRoutine = null;
    }

    void RestoreColor()
    {
        for(int i=0; i<materials.Length; ++i)
        {
            materials[i].color = originalColors[i];
        }
    }
}
