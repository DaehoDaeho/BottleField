using UnityEngine;

/// <summary>
/// 현재 총기 데이터를 기준으로 발사 요청을 해석하고,
/// 발사 가능 여부를 검사하고, 실제 발사를 실행하는 역할.
/// </summary>
public class GunController : MonoBehaviour
{
    [SerializeField] private PlayerCombatInputReader combatInputReader;
    [SerializeField] private PlayerAimController aimController;

    // 발사 방향 기준이 될 카메라 참조.
    [SerializeField] private Camera fireCamera;

    [SerializeField] private GunData currentGunData = new GunData();

    // 현재 프레임 기준 발사 가능 여부.
    [SerializeField] private bool canFire = false;

    // 마지막으로 발사한 게임 시간.
    [SerializeField] private float lastFireTime = -999.0f;

    // 이번 프레임에 발사 요청이 들어왔는지 여부.
    [SerializeField] private bool fireRequested = false;

    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int reserveAmmo = 0;
    [SerializeField] private bool isReloading = false;
    [SerializeField] private float reloadTimer = 0.0f;

    private void Awake()
    {
        fireCamera = Camera.main;

        currentAmmo = currentGunData.magazineSize;
        reserveAmmo = currentGunData.startReserveAmmo;
        isReloading = false;
        reloadTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateReloadTimer();
        // 재장전 입력 처리.
        HandleReloadInput();

        UpdateFireRequest();
        UpdateCanFireState();

        if(fireRequested == true && canFire == true)
        {
            Fire();
        }
    }

    /// <summary>
    /// 현재 총기 데이터의 사격 방식에 따라 이번 프레임 발사 요청 여부를 계산.
    /// </summary>
    void UpdateFireRequest()
    {
        if(currentGunData.isAutomatic == true)
        {
            // 자동 사격 총기라면 발사 버튼을 누르고 있는 동안 계속 발사 요청 상태로 간주.
            fireRequested = combatInputReader.FireHeld;
        }
        else
        {
            // 단발 사격 총기라면 이번 프레임에 눌린 입력만 발사 요청을 간주.
            fireRequested = combatInputReader.FirePressed;
        }
    }

    /// <summary>
    /// 발사 간격을 기준으로 현재 발사 가능 여부를 계산.
    /// </summary>
    void UpdateCanFireState()
    {
        // 현재 게임 진행 시간을 가져온다.
        float currentTime = Time.time;

        // 현재 시간에서 마지막으로 발사했던 시간 사이의 간격을 계산.
        float elapsedTimeSinceLastFire = currentTime - lastFireTime;

        canFire = elapsedTimeSinceLastFire >= (currentGunData.fireInterval * aimController.FireIntervalMultiplier);
    }

    /// <summary>
    /// 실제 발사를 실행.
    /// </summary>
    void Fire()
    {
        if(isReloading == true)
        {
            return;
        }

        if(currentAmmo <= 0)
        {
            return;
        }

        lastFireTime = Time.time;

        Transform cameraTransform = fireCamera.transform;

        Vector3 rayOrigin = cameraTransform.position;
        Vector3 rayDirection = cameraTransform.forward;
        Vector3 rayLength = rayDirection * currentGunData.maxDistance;

        Debug.DrawRay(rayOrigin, rayLength, currentGunData.debugRayColor, 0.2f);

        Ray fireRay = new Ray(rayOrigin, rayDirection);
        RaycastHit hitInfo;

        bool hasHit = Physics.Raycast(fireRay, out hitInfo, currentGunData.maxDistance, currentGunData.hitLayerMask);

        if(hasHit == true)
        {
            ProcessHit(hitInfo, rayDirection);
        }

        currentAmmo--;
    }

    /// <summary>
    /// Raycast 명중 결과 처리.
    /// </summary>
    /// <param name="hitInfo">명중된 대상의 정보</param>
    /// <param name="rayDirection">총알의 진행 방향</param>
    void ProcessHit(RaycastHit hitInfo, Vector3 rayDirection)
    {
        Collider hitCollider = hitInfo.collider;

        string hitObjectName = hitCollider.name;
        float hitDistance = hitInfo.distance;
        Vector3 hitPoint = hitInfo.point;

        IHitTarget hitTarget = hitCollider.GetComponent<IHitTarget>();
        if(hitTarget != null)
        {
            hitTarget.ReceiveHit(currentGunData.damage, hitPoint, rayDirection, hitInfo.normal);
            GameObject effectObject = Instantiate(currentGunData.hitEffectPrefab, hitPoint, Quaternion.identity);
            if(effectObject != null)
            {
                Destroy(effectObject, 1.0f);
            }
        }

        Debug.Log("맞은 놈 이름 = " + hitObjectName);
        Debug.Log("명중 거리 = " + hitDistance);
        Debug.Log("명중 위치 = " + hitPoint);
    }

    void UpdateReloadTimer()
    {
        if(isReloading == false)
        {
            return;
        }

        reloadTimer -= Time.deltaTime;
        if(reloadTimer <= 0.0f)
        {
            // 재장전 완료 처리.
            CompleteReload();
        }
    }

    void CompleteReload()
    {
        int neededAmmo = currentGunData.magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
        reloadTimer = 0.0f;
    }

    void HandleReloadInput()
    {
        if(combatInputReader.ReloadPressed == true)
        {
            // 재장전 시도.
            TryStartReload();
        }
    }

    bool TryStartReload()
    {
        if(isReloading == true)
        {
            return false;
        }

        if(currentAmmo >= currentGunData.magazineSize)
        {
            return false;
        }

        if(reserveAmmo <= 0)
        {
            return false;
        }

        isReloading = true;
        reloadTimer = currentGunData.reloadDuration;
        return true;
    }

    public GunData CurrentGunData
    {
        get { return currentGunData; }
    }

    public bool CanFire
    {
        get { return canFire; }
    }
}
