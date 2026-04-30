using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float currentHealth = 0.0f;

    [SerializeField] private float invincibleDuration = 0.5f;

    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibleTimer = 0.0f;

    public event Action<float, float> OnChangedHealth = null;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInvincibleTimer();
    }

    void UpdateInvincibleTimer()
    {
        if(isInvincible == false)
        {
            return;
        }

        invincibleTimer += Time.deltaTime;

        if(invincibleTimer >= invincibleDuration)
        {
            isInvincible = false;
            invincibleTimer = 0.0f;
        }
    }

    public void ReceiveDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if(isDead == true)
        {
            return;
        }

        if(isInvincible == true)
        {
            return;
        }

        currentHealth -= damageAmount;

        if(currentHealth <= 0.0f)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibleTimer = 0.0f;
        }

        if(OnChangedHealth != null)
        {
            OnChangedHealth(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if(isDead == true)
        {
            return;
        }

        isDead = true;

        isInvincible = false;
        invincibleTimer = 0.0f;
    }

    public void Heal(float healAmount)
    {
        if(isDead == true)
        {
            return;
        }

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        //currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public bool IsDead
    {
        get { return isDead; }
    }
}
