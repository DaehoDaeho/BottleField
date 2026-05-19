using UnityEngine;

[System.Serializable]
public class WeaponRuntimeState
{
    public GunData gunData;

    public int currentAmmo = 0;
    public int reserveAmmo = 0;

    public void Initialize()
    {
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.startReserveAmmo;
    }
}
