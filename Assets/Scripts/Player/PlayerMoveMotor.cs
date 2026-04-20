using UnityEngine;

/// <summary>
/// FPS 플레이어 이동의 기초가 되는 구조를 담당
/// </summary>
public class PlayerMoveMotor : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float jumpSpeed = 6.5f;
    [SerializeField] private float gravity = -20.0f;

    // 바닥에 붙어 있도록 약하게 아래 방향으로 유지할 수직 속도 값.
    [SerializeField] private float groundedStickVelocity = -2.0f;

    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [SerializeField] private Transform moveReference;   // 이동 기준 방향을 제공할 Transform.

    [SerializeField] private CharacterController characterController;

    private Vector2 moveInput = Vector2.zero;

    private Vector3 moveDirection = Vector3.zero;   // 수평 이동 방향 벡터.
    private float verticalVelocity = 0.0f;  // 현재 수직 속도를 저장.

    private bool isGrounded = false;    // 현재 지면에 있는지 여부.

    private Vector3 finalMoveDelta = Vector3.zero;  // 최종 이동량 벡터.

    // Update is called once per frame
    void Update()
    {
        UpdateGroundedState();
        ReadMoveInput();
        UpdateMoveDirection();
        UpdateJumpAndGravity();
        ApplyMovement();
    }

    /// <summary>
    /// CharacterController의 현재 접지 상태를 읽어온다.
    /// </summary>
    void UpdateGroundedState()
    {
        isGrounded = characterController.isGrounded;
    }

    void ReadMoveInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(inputX, inputY).normalized;
    }

    /// <summary>
    /// 현재 입력값을 이동 기준 Transform의 forward/right를 사용해서 월드 이동 방향으로 변환.
    /// </summary>
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

        Vector3 forwardMove = referenceForward * moveInput.y;   // 전후 이동 벡터 계산.
        Vector3 rightMove = referenceRight * moveInput.x;   // 좌우 이동 벡터 계산.
        Vector3 combineMove = (forwardMove + rightMove).normalized;  // 최종 이동 벡터 계산.

        moveDirection = combineMove;
    }

    void UpdateJumpAndGravity()
    {
        if(isGrounded == true && verticalVelocity < 0.0f)
        {
            verticalVelocity = groundedStickVelocity;
        }

        if(isGrounded == true)
        {
            if(Input.GetKeyDown(jumpKey) == true)
            {
                verticalVelocity = jumpSpeed;
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    void ApplyMovement()
    {
        Vector3 horizontalMoveDelta = moveDirection * walkSpeed * Time.deltaTime;
        Vector3 verticalMoveDelta = Vector3.up * verticalVelocity * Time.deltaTime;
        finalMoveDelta = horizontalMoveDelta + verticalMoveDelta;

        characterController.Move(finalMoveDelta);
    }
}
