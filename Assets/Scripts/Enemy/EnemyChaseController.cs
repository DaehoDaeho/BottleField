using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMeshAgent를 사용해 적 캐릭터가 플레이어를 추적하는 역할.
/// </summary>
public class EnemyChaseController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyTargetDetector targetDetector;

    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float angularSpeed = 360.0f;
    [SerializeField] private float acceleration = 8.0f;
    [SerializeField] private float repathInterval = 0.2f;

    [SerializeField] private EnemyAIState currentState = EnemyAIState.Idle;
    [SerializeField] private bool hasPath = false;
    [SerializeField] private float currentAgentSpeed = 0.0f;

    private float repathTimer = 0.0f;

    private void Awake()
    {
        ApplyAgentSettings();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        UpdateMovementByState();
    }

    /// <summary>
    /// NavMeshAgent의 기본 이동 파라미터를 설정값으로 적용.
    /// </summary>
    void ApplyAgentSettings()
    {
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.angularSpeed = angularSpeed;
        navMeshAgent.acceleration = acceleration;

        navMeshAgent.stoppingDistance = targetDetector.StopDistance;
    }

    /// <summary>
    /// 대상 거리 정보를 바탕으로 현재 적 AI 상태를 설정.
    /// </summary>
    void UpdateState()
    {
        if(targetDetector.HasTarget == false)
        {
            currentState = EnemyAIState.Idle;
            return;
        }

        if(targetDetector.IsTargetInChaseRange == false)
        {
            currentState = EnemyAIState.Idle;
            return;
        }

        if(targetDetector.IsTargetInStopDistance == true)
        {
            currentState = EnemyAIState.Stopped;
            return;
        }

        currentState = EnemyAIState.Chasing;
    }

    /// <summary>
    /// 현재 AI의 상태에 따라 이동을 제어.
    /// </summary>
    void UpdateMovementByState()
    {
        if(currentState == EnemyAIState.Idle)
        {
            navMeshAgent.isStopped = true;
            if(navMeshAgent.hasPath == true)
            {
                navMeshAgent.ResetPath();   // 현재 경로가 있으면 경로를 제거.
            }

            return;
        }

        if(currentState == EnemyAIState.Stopped)
        {
            navMeshAgent.isStopped = true;
            if (navMeshAgent.hasPath == true)
            {
                navMeshAgent.ResetPath();   // 현재 경로가 있으면 경로를 제거.
            }

            FaceTargetOnGround();
            return;
        }

        if(currentState == EnemyAIState.Chasing)
        {
            ChaseTarget();
        }
    }

    void ChaseTarget()
    {
        navMeshAgent.isStopped = false;
        repathTimer += Time.deltaTime;

        if(repathTimer < repathInterval)
        {
            return;
        }

        repathTimer = 0.0f;

        navMeshAgent.SetDestination(targetDetector.TargetTransform.position);
    }


    void FaceTargetOnGround()
    {
        Vector3 toTarget = targetDetector.TargetTransform.position - transform.position;
        toTarget.y = 0.0f;

        Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            targetRotation, angularSpeed * Time.deltaTime);
    }

    public EnemyAIState CurrentState
    {
        get { return currentState; }
    }
}
