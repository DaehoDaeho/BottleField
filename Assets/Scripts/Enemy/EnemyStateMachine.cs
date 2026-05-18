using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Dead
}

/// <summary>
/// 적 캐릭터의 행동을 FSM 방식으로 관리하는 역할.
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyTargetDetector targetDetector;
    [SerializeField] private EnemyHealth enemyHealth;

    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float angularSpeed = 360.0f;
    [SerializeField] private float acceleration = 8.0f;
    [SerializeField] private float repathInterval = 0.2f;

    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackInterval = 1.0f;

    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    private EnemyState previousState = EnemyState.Idle;
    private float repathTimer = 0.0f;
    private float lastAttackTime = 0.0f;

    private PlayerHealth cachedPlayerHealth;
    private Transform cachedTargetTransform;

    private void Awake()
    {
        ApplyAgentSettings();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTargetHealthCache();
        DecideState();
        HandleStateChanged();
        RunCurrentState();
    }

    void UpdateTargetHealthCache()
    {
        cachedTargetTransform = targetDetector.TargetTransform;
        cachedPlayerHealth = cachedTargetTransform.GetComponent<PlayerHealth>();
    }

    void ApplyAgentSettings()
    {
        if (CanUseAgent() == false)
        {
            return;
        }

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.angularSpeed = angularSpeed;
        navMeshAgent.acceleration = acceleration;

        navMeshAgent.stoppingDistance = targetDetector.StopDistance;
    }

    /// <summary>
    /// 현재 상황을 바탕으로 적의 상태를 결정한다.
    /// </summary>
    void DecideState()
    {
        if(enemyHealth != null && enemyHealth.IsDead == true)
        {
            currentState = EnemyState.Dead;
            return;
        }

        if(targetDetector == null || targetDetector.HasTarget == false)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if(targetDetector.IsTargetInChaseRange == false)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if(targetDetector.IsTargetInStopDistance == true)
        {
            currentState = EnemyState.Attack;
            return;
        }

        currentState = EnemyState.Chase;
    }

    /// <summary>
    /// 상태가 변경됐을 때 상태 진입 처리를 수행.
    /// </summary>
    void HandleStateChanged()
    {
        if(currentState == previousState)
        {
            return;
        }

        previousState = currentState;

        switch(currentState)
        {
            case EnemyState.Idle:
                {
                    StopAgentAndClearPath();
                }
                break;

            case EnemyState.Chase:
                {
                    StartAgentMovement();
                }
                break;

            case EnemyState.Attack:
                {
                    StopAgentKeepPath();
                }
                break;

            case EnemyState.Dead:
                {
                    StopAgentAndClearPath();
                }
                break;
        }
    }

    /// <summary>
    /// Chase 상태로 진입했을 때 Agent 이동을 허용.
    /// </summary>
    void StartAgentMovement()
    {
        if(CanUseAgent() == false)
        {
            return;
        }

        navMeshAgent.isStopped = false;
    }

    void StopAgentAndClearPath()
    {
        if (CanUseAgent() == false)
        {
            return;
        }

        navMeshAgent.isStopped = true;

        if(navMeshAgent.hasPath == true)
        {
            navMeshAgent.ResetPath();
        }
    }

    void StopAgentKeepPath()
    {
        if (CanUseAgent() == false)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
    }

    void RunCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Chase:
                {
                    RunChaseState();
                }
                break;

            case EnemyState.Attack:
                {
                    RunAttackState();
                }
                break;
        }
    }

    /// <summary>
    /// 일정 시간마다 플레이어 위치를 NavMeshAgent 목적지로 설정.
    /// </summary>
    void RunChaseState()
    {
        if(targetDetector == null || targetDetector.TargetTransform == null)
        {
            return;
        }

        repathTimer += Time.deltaTime;

        if(repathTimer >= repathInterval)
        {
            repathTimer = 0.0f;
            Vector3 targetPosition = targetDetector.TargetTransform.position;
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    void RunAttackState()
    {
        FaceTargetOnGround();
        TryAttackPlayer();
    }

    void FaceTargetOnGround()
    {
        if (targetDetector == null || targetDetector.TargetTransform == null)
        {
            return;
        }

        Vector3 toTarget = targetDetector.TargetTransform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            angularSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 공격 가능한 경우 플레이어에게 데미지 적용.
    /// </summary>
    void TryAttackPlayer()
    {
        if (cachedPlayerHealth.IsDead == true)
        {
            return;
        }

        float currentTime = Time.time;
        float elapsedTime = currentTime - lastAttackTime;

        if(elapsedTime >= attackInterval)
        {
            lastAttackTime = currentTime;
            cachedPlayerHealth.ReceiveDamage(attackDamage, transform.position);
        }
    }

    public void ForceDeadState()
    {
        currentState = EnemyState.Dead;
        HandleStateChanged();
        StopAgentAndClearPath();
    }

    bool CanUseAgent()
    {
        if(navMeshAgent == null)
        {
            return false;
        }

        if(navMeshAgent.enabled == false)
        {
            return false;
        }

        if(navMeshAgent.isOnNavMesh == false)
        {
            return false;
        }

        return true;
    }
}
