using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Dead
}

public enum EnemyAttackMode
{
    Melee,
    Ranged
}

/// <summary>
/// 적 캐릭터의 행동을 FSM 방식으로 관리하는 역할.
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyTargetDetector targetDetector;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Animator animator;

    [SerializeField] private EnemyAttackMode attackMode = EnemyAttackMode.Melee;

    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float angularSpeed = 360.0f;
    [SerializeField] private float acceleration = 8.0f;
    [SerializeField] private float repathInterval = 0.2f;

    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackInterval = 1.0f;

    [Header("원거리 공격 관련")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float rangedAttackDistance = 40.0f;
    [SerializeField] private float rangedAimHeight = 1.2f;
    [SerializeField] private float rangedSpreadAngle = 0.0f;
    [SerializeField] private LayerMask rangedHitLayerMask;

    //[SerializeField] private float minimumSafeDistance = 5.0f;
    //[SerializeField] private float retreatDistance = 4.0f;

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
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if(currentState == EnemyState.Idle)
        {
            animator.SetBool("IsMoving", false);
        }
        else if(currentState == EnemyState.Chase)
        {
            animator.SetBool("IsMoving", true);
        }
        else if(currentState == EnemyState.Attack)  // 임시.
        {
            animator.SetBool("IsMoving", false);
        }
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

        //if (attackMode == EnemyAttackMode.Ranged &&
        //    ShouldRetreatFromTarget() == true)
        //{
        //    currentState = EnemyState.Chase;
        //    return;
        //}

        if (targetDetector.IsTargetInStopDistance == true)
        {
            currentState = EnemyState.Attack;
            return;
        }

        currentState = EnemyState.Chase;
    }

    //private bool ShouldRetreatFromTarget()
    //{
    //    if (attackMode != EnemyAttackMode.Ranged)
    //    {
    //        return false;
    //    }

    //    if (targetDetector == null || targetDetector.TargetTransform == null)
    //    {
    //        return false;
    //    }

    //    float targetDistance = Vector3.Distance(transform.position, targetDetector.TargetTransform.position); // 적과 타겟 사이의 현재 거리를 계산.
    //    bool shouldRetreat = targetDistance < minimumSafeDistance; // 현재 거리가 최소 안전 거리보다 작은지 검사.

    //    return shouldRetreat;
    //}

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

            float targetDistance = Vector3.Distance(transform.position, targetDetector.TargetTransform.position); // 현재 타겟과의 거리를 계산해 저장하는 변수이다.

            //if (targetDistance < minimumSafeDistance)
            //{
            //    MoveAwayFromTarget();
            //    return;
            //}

            Vector3 targetPosition = targetDetector.TargetTransform.position;
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    //private void MoveAwayFromTarget()
    //{
    //    if (CanUseAgent() == false)
    //    {
    //        return;
    //    }

    //    if (targetDetector == null || targetDetector.TargetTransform == null)
    //    {
    //        return;
    //    }

    //    Vector3 fromTarget = transform.position - targetDetector.TargetTransform.position; // 타겟에서 적 방향으로 향하는 벡터를 계산.
    //    fromTarget.y = 0.0f; // 수평 후퇴만 사용하기 위해 Y축 차이를 제거.

    //    if (fromTarget.sqrMagnitude <= 0.0001f)
    //    {
    //        fromTarget = -transform.forward; // 방향 계산이 어려운 경우 현재 전방의 반대 방향을 후퇴 방향으로 사용.
    //    }

    //    Vector3 retreatDirection = fromTarget.normalized; // 후퇴 방향 벡터의 길이를 1로 정규화.
    //    Vector3 desiredPosition = transform.position + (retreatDirection * retreatDistance); // 현재 위치에서 후퇴 방향으로 이동할 목표 위치를 계산.

    //    NavMeshHit navMeshHit; // NavMesh에서 찾은 유효한 위치 정보를 저장할 변수.
    //    bool foundPosition = NavMesh.SamplePosition(desiredPosition, out navMeshHit, 3.0f, navMeshAgent.areaMask); // 원하는 위치 근처에서 NavMesh 위의 유효 위치를 찾는 함수 호출.

    //    if (foundPosition == true)
    //    {
    //        navMeshAgent.SetDestination(navMeshHit.position); // 찾은 NavMesh 위치를 후퇴 목적지로 설정하는 함수 호출.
    //    }
    //}

    void RunAttackState()
    {
        FaceTargetOnGround();

        if(attackMode == EnemyAttackMode.Melee)
        {
            TryAttackPlayer();
        }
        else if(attackMode == EnemyAttackMode.Ranged)
        {
            TryRangedAttackPlayer();
        }
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

    /// <summary>
    /// Raycast 공격이 가능한 경우 FirePoint에서 플레이어 방향으로 Raycast를 발사.
    /// </summary>
    void TryRangedAttackPlayer()
    {
        if(targetDetector == null || targetDetector.TargetTransform == null)
        {
            return;
        }

        float currentTime = Time.time;
        float elapsedTime = currentTime - lastAttackTime;

        if(elapsedTime >= attackInterval)
        {
            Vector3 rayOrigin = firePoint.position;
            Vector3 targetPosition = targetDetector.TargetTransform.position +
                (Vector3.up * rangedAimHeight);
            Vector3 rayDirection = (targetPosition - rayOrigin).normalized;

            lastAttackTime = currentTime;

            Ray attackRay = new Ray(rayOrigin, rayDirection);
            RaycastHit hitInfo;
            bool hasHit = Physics.Raycast(attackRay, out hitInfo, rangedAttackDistance, rangedHitLayerMask, QueryTriggerInteraction.Ignore);

            if(hasHit == true)
            {
                PlayerHealth hitPlayerHealth = hitInfo.collider.GetComponent<PlayerHealth>();

                if(hitPlayerHealth != null && hitPlayerHealth.IsDead == false)
                {
                    hitPlayerHealth.ReceiveDamage(attackDamage, transform.position);
                }
            }
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
