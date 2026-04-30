using UnityEngine;
using UnityEngine.UI;

public class PlayerScreen : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image playerScreenPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth.OnChangedHealth += HandlePlayerScreen;    
    }

    void HandlePlayerScreen(float currentHealth, float maxHealth)
    {
        Color screenColor = playerScreenPanel.color;

        if(currentHealth <= 0.0f)
        {
            screenColor.a = 0.8f;
        }
        else
        {
            float ratio = currentHealth / maxHealth;

            if(ratio >= 0.8f)
            {
                screenColor.a = 0.0f;
            }
            else
            {
                screenColor.a = (1.0f - ratio) * 0.7f;
            }
        }

        playerScreenPanel.color = screenColor;
    }
}
