using UnityEngine;

public class TargetDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 20.0f;
    [SerializeField] private int scoreValue = 10;

    [SerializeField] private GameFlowDemoManager gameFlowDemoManager;

    private float currentHealth = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if(currentHealth <= 0.0f)
        {
            Die();
        }
    }

    void Die()
    {
        if(gameFlowDemoManager != null)
        {
            gameFlowDemoManager.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
