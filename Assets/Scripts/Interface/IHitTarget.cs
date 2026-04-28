using UnityEngine;

/// <summary>
/// 총기나 Raycast 공격에 맞을 수 있는 대상이 구현해야 하는 공통 규칙을 담은 인터페이스.
/// </summary>
public interface IHitTarget
{
    /// <summary>
    /// 총기 Raycast가 명충했을 때 호출.
    /// </summary>
    /// <param name="damage">적용할 데미지 값</param>
    /// <param name="hitPoint">Raycast가 실제로 맞은 월드 좌표</param>
    /// <param name="hitDirection">총알이 날아온 방향</param>
    /// <param name="hitNormal">맞은 표면의 법선 방향</param>
    void ReceiveHit(float damage, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal);
}
