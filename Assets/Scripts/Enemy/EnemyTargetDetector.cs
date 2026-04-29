using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

/// <summary>
/// 적 캐릭터가 추적할 대상을 찾고, 대상과의 거리 정보를 제공하는 스크립트.
/// </summary>
public class EnemyTargetDetector : MonoBehaviour
{
    // 적이 추적할 대상의 Transform.
    [SerializeField] private Transform targetTransform;

    // 대상의 태그 정보.
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool findTargetByTagOnStart = true;

    // 적이 플레이어를 추적 가능한 거리.
    [SerializeField] private float chaseRange = 15.0f;

    [SerializeField] private float stopDistance = 2.0f;

    [SerializeField] private float currentDistanceToTarget = 0.0f;
    [SerializeField] private bool hasTarget = false;    // 추적할 대상을 찾았는지 여부.
    [SerializeField] private bool isTargetInChaseRange = false;
    [SerializeField] private bool isTargetStopDistance = false;

    private void Awake()
    {
        if(targetTransform == null && findTargetByTagOnStart == true)
        {
            // GameObject.FindGameObjectWithTag
            // 씬에 배치된 오브젝트들 중 인자에 해당되는 태그로 지정된 오브젝트를 찾아오는 함수.
            GameObject foundTargetObject = GameObject.FindGameObjectWithTag(targetTag);
            if(foundTargetObject != null)
            {
                targetTransform = foundTargetObject.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTargetDistance();
    }

    /// <summary>
    /// 대상과의 현재 거리를 계산하고 추적 가능 여부를 갱신.
    /// </summary>
    void UpdateTargetDistance()
    {
        hasTarget = targetTransform != null;
        if(hasTarget == false)
        {
            currentDistanceToTarget = 0.0f;
            isTargetInChaseRange = false;
            isTargetStopDistance = false;
            return;
        }

        Vector3 toTarget = targetTransform.position - transform.position;
        float distanceSqr = toTarget.sqrMagnitude;  // 대상까지의 거리 제곱근을 계산.
        float chaseRangeSqr = chaseRange * chaseRange;  // 추적 범위 비교를 위한 추적 거리 제곱값을 계산.
        float stopDistanceSqr = stopDistance * stopDistance;    // 정지 거리 비교를 위한 정지 거리 제곱값 계산.

        currentDistanceToTarget = Mathf.Sqrt(distanceSqr);  // 실제 거리 계산.
        isTargetInChaseRange = distanceSqr <= chaseRangeSqr;    // 추적 가능 여부 갱신.
        isTargetStopDistance = distanceSqr <= stopDistanceSqr;  // 정지 여부 갱신.
    }

    /// <summary>
    /// 외부에서 추적 대상을 직접 설정할때 호출.
    /// </summary>
    /// <param name="newTarget">새로 설정할 추적 대상</param>
    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        UpdateTargetDistance();
    }

    public float StopDistance
    {
        get { return stopDistance; }
    }

    public bool HasTarget
    {
        get { return hasTarget; }
    }

    public bool IsTargetInChaseRange
    {
        get { return isTargetInChaseRange; }
    }

    public bool IsTargetInStopDistance
    {
        get { return isTargetStopDistance; }
    }

    public Transform TargetTransform
    {
        get { return targetTransform; }
    }
}
