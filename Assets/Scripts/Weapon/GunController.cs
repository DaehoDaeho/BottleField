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

    [SerializeField] private WeaponRuntimeState[] weaponSlots;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private int currentWeaponIndex = 0;

    // 현재 프레임 기준 발사 가능 여부.
    [SerializeField] private bool canFire = false;

    // 마지막으로 발사한 게임 시간.
    [SerializeField] private float lastFireTime = -999.0f;

    // 이번 프레임에 발사 요청이 들어왔는지 여부.
    [SerializeField] private bool fireRequested = false;

    [SerializeField] private bool isReloading = false;
    [SerializeField] private float reloadTimer = 0.0f;

    private void Awake()
    {
        fireCamera = Camera.main;

        isReloading = false;
        reloadTimer = 0.0f;

        InitializeWeaponSlots();
        SelectWeapon(0);
    }

    // Update is called once per frame
    void Update()
    {
        HandleWeaponSwitchInput();
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
    /// 모든 무기 슬롯의 탄약 상태를 초기화.
    /// </summary>
    void InitializeWeaponSlots()
    {
        weaponSlots = new WeaponRuntimeState[weaponData.GetGunDatasCount()];

        for(int i=0; i< weaponSlots.Length; ++i)
        {
            weaponSlots[i] = new WeaponRuntimeState();
            weaponSlots[i].gunData = weaponData.GetGunDataByIndex(i);
            if(weaponSlots[i].gunData != null)
            {
                weaponSlots[i].Initialize();
            }
        }
    }

    public void SelectWeapon(int weaponIndex)
    {
        currentWeaponIndex = weaponIndex;
        isReloading = false;
        reloadTimer = 0.0f;
    }

    void HandleWeaponSwitchInput()
    {
        if(combatInputReader.WeaponOnePressed == true)
        {
            SelectWeapon(0);
        }

        if (combatInputReader.WeaponTwoPressed == true)
        {
            SelectWeapon(1);
        }

        if (combatInputReader.WeaponThreePressed == true)
        {
            SelectWeapon(2);
        }
    }

    /// <summary>
    /// 현재 총기 데이터의 사격 방식에 따라 이번 프레임 발사 요청 여부를 계산.
    /// </summary>
    void UpdateFireRequest()
    {
        GunData gunData = weaponSlots[currentWeaponIndex].gunData;

        if(gunData.isAutomatic == true)
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

        canFire = elapsedTimeSinceLastFire >= (CurrentGunData.fireInterval * aimController.FireIntervalMultiplier);
    }

    /// <summary>
    /// 실제 발사를 실행.
    /// </summary>
    void Fire()
    {
        WeaponRuntimeState currentSlot = weaponSlots[currentWeaponIndex];
        GunData gunData = currentSlot.gunData;

        if(isReloading == true)
        {
            return;
        }

        if(currentSlot.currentAmmo <= 0)
        {
            return;
        }

        lastFireTime = Time.time;

        Transform cameraTransform = fireCamera.transform;

        Vector3 rayOrigin = cameraTransform.position;
        Vector3 rayDirection = cameraTransform.forward;
        Vector3 rayLength = rayDirection * CurrentGunData.maxDistance;

        int pelletCount = gunData.pelletCount;

        for(int i=0; i<pelletCount; ++i)
        {
            Vector3 shotDirection = CalculateShotDirection(rayDirection, gunData);

            Debug.DrawRay(rayOrigin, rayLength, gunData.debugRayColor, 0.2f);

            Ray fireRay = new Ray(rayOrigin, shotDirection);
            RaycastHit hitInfo;

            bool hasHit = Physics.Raycast(fireRay, out hitInfo,
                gunData.maxDistance, gunData.hitLayerMask);

            if (hasHit == true)
            {
                ProcessHit(hitInfo, shotDirection);
            }
        }

        currentSlot.currentAmmo--;
    }

    Vector3 CalculateShotDirection(Vector3 baseDirection, GunData gunData)
    {
        if(gunData.pelletCount == 1)
        {
            return baseDirection;
        }

        if(gunData.spreadAngle <= 0.0f)
        {
            return baseDirection;
        }

        float randomPitch = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        float randomYaw = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(randomPitch, randomYaw, 0.0f);
        Vector3 spreadDirection = spreadRotation * baseDirection;

        return spreadDirection.normalized;
    }

    /// <summary>
    /// Raycast 명중 결과 처리.
    /// </summary>
    /// <param name="hitInfo">명중된 대상의 정보</param>
    /// <param name="rayDirection">총알의 진행 방향</param>
    void ProcessHit(RaycastHit hitInfo, Vector3 rayDirection)
    {
        GunData gunData = weaponSlots[currentWeaponIndex].gunData;

        Collider hitCollider = hitInfo.collider;

        string hitObjectName = hitCollider.name;
        float hitDistance = hitInfo.distance;
        Vector3 hitPoint = hitInfo.point;

        IHitTarget hitTarget = hitCollider.GetComponent<IHitTarget>();
        if(hitTarget != null)
        {
            hitTarget.ReceiveHit(gunData.damage, hitPoint, rayDirection, hitInfo.normal);

            if(gunData.hitEffectPrefab != null)
            {
                GameObject effectObject = Instantiate(gunData.hitEffectPrefab, hitPoint, Quaternion.identity);
                if (effectObject != null)
                {
                    Destroy(effectObject, 1.0f);
                }
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
        WeaponRuntimeState currentSlot = weaponSlots[currentWeaponIndex];
        GunData gunData = weaponSlots[currentWeaponIndex].gunData;

        int neededAmmo = gunData.magazineSize - currentSlot.currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, currentSlot.reserveAmmo);

        currentSlot.currentAmmo += ammoToLoad;
        currentSlot.reserveAmmo -= ammoToLoad;

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
        WeaponRuntimeState currentSlot = weaponSlots[currentWeaponIndex];
        GunData gunData = weaponSlots[currentWeaponIndex].gunData;

        if (isReloading == true)
        {
            return false;
        }

        if(currentSlot.currentAmmo >= gunData.magazineSize)
        {
            return false;
        }

        if(currentSlot.reserveAmmo <= 0)
        {
            return false;
        }

        isReloading = true;
        reloadTimer = gunData.reloadDuration;
        return true;
    }

    public GunData CurrentGunData
    {
        get { return weaponSlots[currentWeaponIndex].gunData; }
    }

    public bool CanFire
    {
        get { return canFire; }
    }

    public int CurrentAmmo
    {
        get { return weaponSlots[currentWeaponIndex].currentAmmo; }
    }

    public int ReserveAmmo
    {
        get { return weaponSlots[currentWeaponIndex].reserveAmmo; }
    }

    public bool IsReloading
    {
        get { return isReloading; }
    }
}
