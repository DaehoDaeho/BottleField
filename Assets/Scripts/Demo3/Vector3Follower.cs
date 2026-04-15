using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 목표 오브젝트를 향한 방향 벡터 계산,
/// 거리 계산, 정규화, 회전, 이동을 실제 동작으로 보여주는 역할.
/// </summary>
public class Vector3Follower : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;

    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float stopDistance = 1.0f;
    [SerializeField] private bool useOnlyYawRotation = true;

    [SerializeField] private float rotationLerpSpeed = 8.0f;

    private Vector3 rawDirection = Vector3.zero;
    private Vector3 normalizedDirection = Vector3.zero;

    private float currentDistance = 0.0f;
    public float CurrentDistance
    {
        get
        {
            return currentDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDirectionData();
        RotateTowardTarget();
        MoveTowardTarget();
    }

    /// <summary>
    /// 현재 위치와 목표 위치를 바탕으로 방향 벡터와 거리 계산.
    /// </summary>
    void UpdateDirectionData()
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = targetTransform.position;

        rawDirection = targetPosition - currentPosition;
        currentDistance = rawDirection.magnitude;

        if(currentDistance > 0.0001f)
        {
            normalizedDirection = rawDirection.normalized;
        }
        else
        {
            normalizedDirection = Vector3.zero; // 목표와 거의 같은 위치일 경우.
        }
    }

    /// <summary>
    /// 목표 방향을 바라보도록 회전.
    /// </summary>
    void RotateTowardTarget()
    {
        Vector3 lookDirection = rawDirection;

        if(useOnlyYawRotation == true)
        {
            lookDirection.y = 0.0f;
        }

        Vector3 normalizedLookDirection = lookDirection.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(normalizedLookDirection, Vector3.up);
        Quaternion currentRotation = transform.rotation;
        Quaternion nextRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationLerpSpeed * Time.deltaTime);

        transform.rotation = nextRotation;
    }

    /// <summary>
    /// 정규화된 방향 벡터를 이용해서 목표를 향해 일정 속도로 이동.
    /// </summary>
    void MoveTowardTarget()
    {
        if(currentDistance <= stopDistance)
        {
            return;
        }

        Vector3 moveDelta = normalizedDirection * moveSpeed * Time.deltaTime;
        transform.position += moveDelta;
    }
}
