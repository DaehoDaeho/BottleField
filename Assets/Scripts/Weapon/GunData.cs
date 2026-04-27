using UnityEngine;

/// <summary>
/// 총기의 기본 설정값을 보관하는 데이터 클래스.
/// </summary>
[System.Serializable]
public class GunData
{
    public string gunName = "Pistol";

    // 자동 사격 가능한 총인지 여부.
    public bool isAutomatic = false;

    // 발사 간격.
    public float fireInterval = 0.25f;

    // 사거리.
    public float maxDistance = 100.0f;

    public Color debugRayColor = Color.yellow;
}
