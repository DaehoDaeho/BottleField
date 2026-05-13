using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 화면에 표시되는 기본 HUD를 관리하는 역할.
/// </summary>
public class HUDUIController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GunController gunController;
    [SerializeField] private PlayerAimController aimController;

    [Header("Health UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthFillImage;

    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;

    [Header("Crosshair UI")]
    [SerializeField] private RectTransform crosshairRoot;
    [SerializeField] private float normalCrosshairScale = 1.0f;
    [SerializeField] private float aimCrosshairScale = 0.75f;
    [SerializeField] private float crosshairScaleLerpSpeed = 12.0f;

    /// <summary>
    /// 시작 시 기본 UI 상태를 한 번 갱신한다.
    /// </summary>
    private void Start()
    {
        UpdateHealthUi();
        UpdateAmmoUi();
        UpdateReloadUi();
        UpdateCrosshairUi();
    }

    /// <summary>
    /// 매 프레임 HUD 정보를 갱신한다.
    /// </summary>
    private void Update()
    {
        UpdateHealthUi();
        UpdateAmmoUi();
        UpdateReloadUi();
        UpdateCrosshairUi();
    }

    /// <summary>
    /// 플레이어 체력 텍스트와 체력바를 갱신한다..
    /// </summary>
    private void UpdateHealthUi()
    {
        if (playerHealth == null)
        {
            return;
        }

        float currentHealth = playerHealth.CurrentHealth;
        float maxHealth = playerHealth.MaxHealth;
        float healthPercent = 0.0f;

        if (maxHealth > 0.0f)
        {
            healthPercent = currentHealth / maxHealth;
        }

        healthPercent = Mathf.Clamp01(healthPercent);

        if (healthText != null)
        {
            healthText.text = "HP " + currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercent;
        }
    }

    /// <summary>
    /// 총기 탄약 UI를 갱신한다.
    /// </summary>
    private void UpdateAmmoUi()
    {
        if (gunController == null)
        {
            return;
        }

        int currentAmmo = gunController.CurrentAmmo;
        int reserveAmmo = gunController.ReserveAmmo;

        if (ammoText != null)
        {
            ammoText.text = "Ammo " + currentAmmo.ToString() + " / " + reserveAmmo.ToString();
        }
    }

    /// <summary>
    /// 재장전 상태 UI를 갱신한다.
    /// </summary>
    private void UpdateReloadUi()
    {
        if (reloadText == null)
        {
            return;
        }

        if (gunController == null)
        {
            reloadText.gameObject.SetActive(false);
            return;
        }

        bool isReloading = gunController.IsReloading;

        reloadText.gameObject.SetActive(isReloading);

        if (isReloading == true)
        {
            reloadText.text = "Reloading...";
        }
    }

    /// <summary>
    /// 크로스헤어 UI를 갱신한다.
    /// </summary>
    private void UpdateCrosshairUi()
    {
        if (crosshairRoot == null)
        {
            return;
        }

        bool shouldShowCrosshair = true;

        if (playerHealth != null && playerHealth.IsDead == true)
        {
            shouldShowCrosshair = false;
        }

        crosshairRoot.gameObject.SetActive(shouldShowCrosshair);

        if (shouldShowCrosshair == false)
        {
            return;
        }

        float targetScale = normalCrosshairScale;

        if (aimController != null && aimController.IsAiming == true)
        {
            targetScale = aimCrosshairScale;
        }

        Vector3 currentScale = crosshairRoot.localScale;
        Vector3 targetScaleVector = Vector3.one * targetScale;
        Vector3 nextScale = Vector3.Lerp(currentScale, targetScaleVector, crosshairScaleLerpSpeed * Time.deltaTime);

        crosshairRoot.localScale = nextScale;
    }
}