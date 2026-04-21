using UnityEngine;

/// <summary>
/// 입력 리더가 제공하는 현재 입력 상태를 읽어서 실제 이동, 점프, 질주, 앉기 속도 처리, 중력 처리를 담당.
/// </summary>
public class PlayerLocomotionController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStanceController stanceController;
    [SerializeField] private Transform moveReference;

    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [SerializeField] private float jumpSpeed = 6.5f;
    [SerializeField] private float gravity = -20.0f;
    [SerializeField] private float groundedStickVelocity = -2.0f;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 finalMoveDelta = Vector3.zero;
    private float verticalVelocity = 0.0f;
    private float currentMoveSpeed = 0.0f;
    private bool isGrounded = false;
    private bool isSprinting = false;

    // Update is called once per frame
    void Update()
    {
        UpdateGroundedState();
        UpdateSpeedState();
        UpdateMoveDirection();
        UpdateJumpAndGravity();
        ApplyMovement();
    }

    void UpdateGroundedState()
    {
        isGrounded = characterController.isGrounded;
    }

    /// <summary>
    /// 현재 입력 상태와 자세 상태를 바탕으로 이동 속도를 설정.
    /// </summary>
    void UpdateSpeedState()
    {
        bool isCrouching = stanceController.IsCrouching == true;
        bool wantsSprint = inputReader.SprintHeld == true;
        bool hasMoveInput = inputReader.MoveInput.sqrMagnitude > 0.0f;

        isSprinting = wantsSprint == true && isCrouching == false && hasMoveInput == true;

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

    void UpdateMoveDirection()
    {
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

        moveDirection = combineMove;
    }

    void UpdateJumpAndGravity()
    {
        if (isGrounded == true && verticalVelocity < 0.0f)
        {
            verticalVelocity = groundedStickVelocity;
        }

        if(inputReader != null)
        {
            if(isGrounded == true && inputReader.JumpPressed == true)
            {
                bool isCrouching = stanceController.IsCrouching == true;

                if(isCrouching == false)
                {
                    verticalVelocity = jumpSpeed;
                }

                inputReader.ConsumeJumpPressed();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    void ApplyMovement()
    {
        Vector3 horizontalMoveDelta = moveDirection * currentMoveSpeed * Time.deltaTime;
        Vector3 verticalMoveDelta = Vector3.up * verticalVelocity * Time.deltaTime;
        finalMoveDelta = horizontalMoveDelta + verticalMoveDelta;

        characterController.Move(finalMoveDelta);
    }
}
