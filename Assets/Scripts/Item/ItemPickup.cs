using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemPickType itemType = ItemPickType.Health;
    [SerializeField] private float healthAmount = 20.0f;
    [SerializeField] private int ammoAmount = 30;

    [SerializeField] private bool respawnAfterUse = false;
    [SerializeField] private float respawnDelay = 15.0f;

    [SerializeField] private GameObject VisualRoot;
    [SerializeField] private float rotateSpeed = 90.0f;

    [SerializeField] private Collider pickupCollider;

    private bool isAvailable = true;
    private float respawnTimer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        RotateVisual();
        UpdateRespawnTimer();
    }

    void RotateVisual()
    {
        if(isAvailable == false)
        {
            return;
        }

        if(VisualRoot == null)
        {
            return;
        }

        float rotationAmount = rotateSpeed * Time.deltaTime;
        VisualRoot.transform.Rotate(0.0f, rotationAmount, 0.0f, Space.World);
    }

    void UpdateRespawnTimer()
    {
        if(respawnAfterUse == false)
        {
            return;
        }

        if(isAvailable == true)
        {
            return;
        }

        respawnTimer += Time.deltaTime;

        if(respawnTimer >= respawnDelay)
        {
            // 아이템 활성화.
            ShowPickup();
        }
    }

    void ShowPickup()
    {
        isAvailable = true;
        respawnTimer = 0.0f;

        if(VisualRoot != null)
        {
            VisualRoot.SetActive(true);
        }

        if(pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }
    }

    void ConsumePickup()
    {
        isAvailable = false;
        respawnTimer = 0.0f;

        if(VisualRoot != null)
        {
            VisualRoot.SetActive(false);
        }

        if(pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        if(respawnAfterUse == false)
        {
            gameObject.SetActive(false);
        }
    }

    bool TryApplyToPlayer(Collider other)
    {
        if(itemType == ItemPickType.Health)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if(playerHealth != null)
            {
                playerHealth.Heal(healthAmount);
                return true;
            }
        }
        else if(itemType == ItemPickType.Ammo)
        {
            GunController gunController = other.GetComponent<GunController>();
            if(gunController != null)
            {
                gunController.AddReserveAmmo(ammoAmount);
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isAvailable == false)
        {
            return;
        }

        bool wasApplied = TryApplyToPlayer(other);
        if(wasApplied == true)
        {
            ConsumePickup();
        }
    }
}
