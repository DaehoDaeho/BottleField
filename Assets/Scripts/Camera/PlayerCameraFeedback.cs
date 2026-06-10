using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraFeedback : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GunController gunController;
    [SerializeField] private Transform cameraReference;

    [SerializeField] private CinemachineImpulseSource weaponFireImpulseSource;
    [SerializeField] private CinemachineImpulseSource damageImpulseSource;

    [SerializeField] private float weaponFireImpulseForce = 0.2f;
    [SerializeField] private float damageImpulseForce = 1.0f;
    [SerializeField] private float verticalDamageInfluence = 0.2f;

    void OnEnable()
    {
        playerHealth.Damaged += HandlePlayerDamaged;
        gunController.WeaponFired += HandleWeaponFired;
    }

    void OnDisable()
    {
        playerHealth.Damaged -= HandlePlayerDamaged;
        gunController.WeaponFired -= HandleWeaponFired;
    }

    void HandleWeaponFired()
    {
        weaponFireImpulseSource.GenerateImpulseWithForce(weaponFireImpulseForce);
    }

    void HandlePlayerDamaged(float appliedDamage, Vector3 damageSourcePosition)
    {
        Vector3 impulseDirection = CalculateDamageImpulseDirection(damageSourcePosition);
        float impulseForce = CalculateDamageImpulseForce(appliedDamage);
        Vector3 impulseVelocity = impulseDirection * impulseForce;

        damageImpulseSource.GenerateImpulseAtPositionWithVelocity(transform.position, impulseVelocity);
    }

    /// <summary>
    /// 공격 위치를 기준으로 카메라가 움직일 방향을 계산.
    /// </summary>
    /// <param name="damageSourcePosition"></param>
    /// <returns></returns>
    Vector3 CalculateDamageImpulseDirection(Vector3 damageSourcePosition)
    {
        Vector3 referencePosition = transform.position;

        Vector3 awayFromDamage = referencePosition - damageSourcePosition;
        awayFromDamage.y += verticalDamageInfluence;

        if(awayFromDamage.sqrMagnitude <= 0.0001f)
        {
            awayFromDamage = cameraReference.forward;
        }

        return awayFromDamage.normalized;
    }

    float CalculateDamageImpulseForce(float appliedDamage)
    {
        float safeMaximumDamage = 30.0f;
        float damageRatio = appliedDamage / safeMaximumDamage;

        // Mathf.Clamp01 : 0~1 사이의 범위를 벗어나지 않도록 보정시켜주는 함수.
        damageRatio = Mathf.Clamp01(damageRatio);

        float safeMinimumForce = Mathf.Max(0.0f, 0.3f);
        float safeMaximumForce = Mathf.Max(safeMinimumForce, 1.0f);

        // 데미지 비율을 기준으로 최소와 최대 흔들림 강도 사이 값을 계산.
        float calculatedForce = Mathf.Lerp(safeMinimumForce, safeMaximumForce, damageRatio);

        return calculatedForce;
    }
}
