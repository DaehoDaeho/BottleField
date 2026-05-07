using UnityEngine;

/// <summary>
/// 플레이어의 조준 상태와 조준 중 카메라 FOV 변화를 담당하는 역할.
/// </summary>
public class PlayerAimController : MonoBehaviour
{
    [SerializeField] private PlayerCombatInputReader combatInputReader;
    [SerializeField] private Camera targetCamera;

    [SerializeField] private float normalFov = 60.0f;
    [SerializeField] private float aimFov = 30.0f;
    [SerializeField] private float fovLerpSpeed = 10.0f;

    [SerializeField] private float aimMoveSpeedMultiplier = 0.55f;
    [SerializeField] private float aimFireIntervalMultiplier = 1.15f;

    [SerializeField] private bool isAiming = false;

    private void Awake()
    {
        if(targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if(targetCamera != null)
        {
            targetCamera.fieldOfView = normalFov;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAimState();
        UpdateCameraFov();
    }

    /// <summary>
    /// 입력 상태를 읽어 현재 조준 여부를 결정.
    /// </summary>
    void UpdateAimState()
    {
        isAiming = combatInputReader.AimHeld;
    }

    /// <summary>
    /// 현재 조준 상태에 따라 카메라 FOV를 부드럽게 변경.
    /// </summary>
    void UpdateCameraFov()
    {
        float targetFov = isAiming == true ? aimFov : normalFov;
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
    }

    public bool IsAiming
    {
        get { return isAiming; }
    }

    public float MoveSpeedMultiplier
    {
        get
        {
            if(isAiming == true)
            {
                return aimFireIntervalMultiplier;
            }

            return 1.0f;
        }
    }

    public float FireIntervalMultiplier
    {
        get
        {
            if(isAiming == true)
            {
                return aimFireIntervalMultiplier;
            }

            return 1.0f;
        }
    }
}
