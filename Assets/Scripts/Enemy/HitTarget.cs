using System.Collections;
using UnityEngine;

/// <summary>
/// GunController로부터 데미지와 명중 정보를 전달 받아서 명중 처리를 한다.
/// </summary>
public class HitTarget : MonoBehaviour, IHitTarget
{
    [SerializeField] private float maxHealth = 30.0f;
    [SerializeField] private float currentHealth = 0.0f;

    // 체력이 0 이하가 됐을 때 이 오브젝트를 비활성화 할지 여부.
    [SerializeField] private bool disableOnDeath = false;
    [SerializeField] private bool isDead = false;

    [SerializeField] private Renderer renderer;
    [SerializeField] private Material mat;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitColorDuration = 0.2f;

    private Color baseColor;
    Coroutine hitColorRoutine = null;

    private void Awake()
    {
        currentHealth = maxHealth;

        mat = renderer.material;
        baseColor = mat.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReceiveHit(float damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        if(isDead == true)
        {
            return;
        }

        if (hitColorRoutine != null)
        {
            StopCoroutine(hitColorRoutine);
        }
        hitColorRoutine = StartCoroutine(HitColorRoutine());

        currentHealth -= damage;

        if(currentHealth <= 0.0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0.0f;

        if(hitColorRoutine != null)
        {
            StopCoroutine(hitColorRoutine);
        }

        if(disableOnDeath == true)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator HitColorRoutine()
    {
        mat.color = hitColor;

        yield return new WaitForSeconds(hitColorDuration);

        mat.color = baseColor;
    }
}
