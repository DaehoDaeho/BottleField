using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 입력 리더가 제공하는 현재 입력 상태를 읽어서 실제 이동, 점프, 질주, 앉기 속도 처리, 중력 처리를 담당.
/// </summary>
public class PlayerLocomotionController : MonoBehaviour
{
    [Header("참조 설정")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStanceController stanceController;
    [SerializeField] private Transform moveReference;
    [SerializeField] private Transform cameraBobTarget; // 이동 시 흔들림을 적용할 대상의 Transform.

    [Header("속도 설정")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("가속도 설정")]
    [SerializeField] private float groundAcceleration = 25.0f;  // 지상에서의 수평 속도를 목표 속도로 따라가게 만들 가속 값.
    [SerializeField] private float airAcceleration = 1.0f;  // 공중에서의 수평 속도를 목표 속도로 따라가게 만들 가속 값.

    [Header("중력 설정")]
    [SerializeField] private float jumpSpeed = 6.5f;
    [SerializeField] private float gravity = -20.0f;
    [SerializeField] private float groundedStickVelocity = -2.0f;

    [SerializeField] private float bobFrequency = 10.0f;    // 이동 시 흔들림 진동파가 진행되는 기본 속도 값.
    [SerializeField] private float walkBobAmplitude = 0.03f;    // 걷기 상태에서 적용할 위아래 흔들림 진폭 값.
    [SerializeField] private float sprintBobAmplitude = 0.06f;  // 질주 상태에서 적용할 위아래 흔들림 진폭 값.
    [SerializeField] private float crouchBobAmplitude = 0.015f; // 앉기 상태에서 적용할 위아래 흔들림 진폭 값.
    
    // 현재 이동 시 흔들림 오프셋을 목표 오프셋 쪽으로 따라가게 만들 보간 속도.
    [SerializeField] private float bobLerpSpeed = 12.0f;

    private Vector3 desiredMoveDirection = Vector3.zero;    // 현재 입력 기준으로 플레이어가 가고 싶어하는 수평 방향 벡터.
    private Vector3 currentHorizontalVelocity = Vector3.zero;   // 실제로 적용 중인 현재 수평 속도 벡터.

    private Vector3 moveDirection = Vector3.zero;
    
    private Vector3 finalMoveDelta = Vector3.zero;
    private float verticalVelocity = 0.0f;
    private float currentMoveSpeed = 0.0f;

    private bool isGrounded = false;
    private bool isSprinting = false;
    private bool isCrouching = false;

    private float bobTimer = 0.0f;  // 이동 시 흔들림 진동의 진행 시간을 누적하기 위한 변수.
    private float currentBobOffsetY = 0.0f; // 현재 적용중인 이동 흔들림 Y 오프셋 값.
    private float baseBobLocalY = 0.0f; // cameraBobTarget의 기본 로컬 Y 위치 값.

    private void Awake()
    {
        if(cameraBobTarget != null)
        {
            baseBobLocalY = cameraBobTarget.localPosition.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRuntimeStates();
        UpdateCurrentMoveState();
        UpdateDesiredMoveDirection();
        UpdateHorizontalVelocity();
        UpdateJumpAndGravity();
        ApplyMovement();
        UpdateHeadBob();
    }

    /// <summary>
    /// 현재 착지 상태, 앉기 상태, 달리기 상태를 갱신.
    /// </summary>
    void UpdateRuntimeStates()
    {
        isGrounded = characterController.isGrounded;
        isCrouching = stanceController != null && stanceController.IsCrouching == true;

        bool hasMoveInput = inputReader != null && inputReader.MoveInput.sqrMagnitude > 0.0f;
        bool wantsSprint = inputReader != null && inputReader.SprintHeld == true;

        isSprinting = wantsSprint == true && isCrouching == false && isGrounded == true && hasMoveInput == true;
    }

    /// <summary>
    /// 현재 상태에 따라 목표 이동 속도를 결정.
    /// </summary>
    void UpdateCurrentMoveState()
    {        
        if(isCrouching == true)
        {
            currentMoveSpeed = crouchSpeed;
        }
        else if(isSprinting == true)
        {
            currentMoveSpeed = sprintSpeed;
        }
        else
        {
            currentMoveSpeed = walkSpeed;
        }
    }

    /// <summary>
    /// 입력 리더의 이동 입력을 바탕으로 시점 기준 원하는 수평 이동 방향을 계산.
    /// </summary>
    void UpdateDesiredMoveDirection()
    {
        if(inputReader == null)
        {
            desiredMoveDirection = Vector3.zero;
            return;
        }

        Vector2 moveInput = inputReader.MoveInput;  // 현재 이동 입력 벡터를 읽어온다.
        Vector3 referenceForward = moveReference.forward;
        Vector3 referenceRight = moveReference.right;

        // 수평 이동만 사용하기 위해 앞/오른쪽 방향 벡터의 Y 성분을 제거.
        referenceForward.y = 0.0f;
        referenceRight.y = 0.0f;

        // Y 성분 제거 후 길이가 달라질 수 있으므로 벡터의 길이를 1로 만들어준다.(정규화)
        referenceForward = referenceForward.normalized;
        referenceRight = referenceRight.normalized;

        Vector3 forwardMove = referenceForward * inputReader.MoveInput.y;   // 전후 이동 벡터 계산.
        Vector3 rightMove = referenceRight * inputReader.MoveInput.x;   // 좌우 이동 벡터 계산.
        Vector3 combineMove = (forwardMove + rightMove).normalized;  // 최종 이동 벡터 계산.

        desiredMoveDirection = combineMove;
    }

    /// <summary>
    /// 원하는 이동 방향과 현재 목표 속도를 기준으로 실제 수평 속도를 갱신.
    /// 지상과 공중에서 서로 다른 가속 값을 사용.
    /// </summary>
    void UpdateHorizontalVelocity()
    {
        // 목표 수평 속도 = 원하는 이동 방향 * 현재 목표 이동 속도.
        Vector3 targetHorizontalVelocity = desiredMoveDirection * currentMoveSpeed;
        float currentAcceleration = isGrounded ? groundAcceleration : airAcceleration;

        // 현재 프레임의 실제 수평 속도가 목표 수평 속도 쪽으로 얼마나 따라갈지 결정하는 보간 계수 계산.
        float lerpFactor = currentAcceleration * Time.deltaTime;

        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetHorizontalVelocity, lerpFactor);
    }

    void UpdateJumpAndGravity()
    {
        if (isGrounded == true && verticalVelocity < 0.0f)
        {
            verticalVelocity = groundedStickVelocity;
        }

        if(inputReader != null)
        {
            if(isGrounded == true && isCrouching == false && inputReader.JumpPressed == true)
            {
                verticalVelocity = jumpSpeed;
                inputReader.ConsumeJumpPressed();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    void ApplyMovement()
    {
        Vector3 horizontalMoveDelta = currentHorizontalVelocity * Time.deltaTime;
        Vector3 verticalMoveDelta = Vector3.up * verticalVelocity * Time.deltaTime;
        finalMoveDelta = horizontalMoveDelta + verticalMoveDelta;

        characterController.Move(finalMoveDelta);
    }

    /// <summary>
    /// 현재 이동 상태에 따라 흔들림을 계산하고 cameraBobTarget의 로컬 위치에 반영.
    /// </summary>
    void UpdateHeadBob()
    {
        if(cameraBobTarget == null)
        {
            return;
        }

        // 현재 실제 수평 속도에서 x,z 성분만 추출한 벡터를 만드는 계산.
        Vector3 horizontalVelocityOnly = new Vector3(currentHorizontalVelocity.x, 0.0f, currentHorizontalVelocity.z);

        // 실제 수평 이동 속도의 크기를 계산.
        float horizontalSpeed = horizontalVelocityOnly.magnitude;

        // 지상에 있고 실제 수평 속도가 충분히 있을때만 흔들림을 적용하기 위한 판단.
        bool isMovingOnGround = isGrounded == true && horizontalSpeed > 0.1f;

        float targetBobOffsetY = 0.0f;

        // 지상에서 이동 중일 경우.
        if(isMovingOnGround == true)
        {
            float currentBobAmplitude = walkBobAmplitude;

            if(isCrouching == true)
            {
                currentBobAmplitude = crouchBobAmplitude;
            }
            else if(isSprinting == true)
            {
                currentBobAmplitude = sprintBobAmplitude;
            }

            // 실제 수평 속도를 현재 목표 이동 속도로 나눠 이동 강도 비율을 계산.
            float speedRatio = currentMoveSpeed > 0.0f ? horizontalSpeed / currentMoveSpeed : 0.0f;

            // 이동 강도 비율이 0~1 범위를 넘지 않도록 보정.
            float clampedSpeedRatio = Mathf.Clamp01(speedRatio);

            // 현재 이동 강도에 따라 흔들림 진행 속도를 계산.
            float bobSpeed = bobFrequency * clampedSpeedRatio;

            bobTimer += bobSpeed * Time.deltaTime;  // 이동 흔들림 진동파 진행 시간을 현재 흔들림 속도에 따라 누적하는 계산.

            // 현재 이동 흔들림 진동파(사인파) 값을 계산.
            float waveValue = Mathf.Sin(bobTimer);

            // 진동파 값에 현재 상태 진폭을 곱해서 목표 Y 오프셋을 계산.
            targetBobOffsetY = waveValue * currentBobAmplitude;
        }
        else
        {
            bobTimer = 0.0f;
            targetBobOffsetY = 0.0f;
        }

        // 현재 이동 흔들림 오프셋을 목표 오프셋 쪽으로 부드럽게 보간.
        currentBobOffsetY = Mathf.Lerp(currentBobOffsetY, targetBobOffsetY, bobLerpSpeed * Time.deltaTime);

        // 현재 cameraBobTarget의 로컬 위치값을 가져온다.
        Vector3 currentLocalPosition = cameraBobTarget.localPosition;

        // 기본 카메라 높이에 현재 이동 흔들림 오프셋을 더한 새 로컬 위치 계산.
        Vector3 nextLocalPosition = new Vector3(currentLocalPosition.x, baseBobLocalY + currentBobOffsetY, currentLocalPosition.z);

        // 계산된 새 로컬 위치를 cameraBobTarget의에 적용.
        cameraBobTarget.localPosition = nextLocalPosition;
    }
}
