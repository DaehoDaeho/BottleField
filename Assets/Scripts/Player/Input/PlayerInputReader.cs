using UnityEngine;

/// <summary>
/// 플레이어 입력을 한곳에서 읽고 정리하는 역할.
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift; // 질주.
    [SerializeField] private KeyCode crouchToggleKey = KeyCode.LeftControl; // 앉기.

    [SerializeField] private bool enableInput = true;   // 현재 플레이어 입력을 허용할지 여부.

    private Vector2 moveInput = Vector2.zero;
    private bool jumpPressed = false;   // 점프 입력이 눌렸는지 여부.
    private bool sprintHeld = false;    // 질주 키를 누르고 있는지 여부.
    private bool crouchTogglePressed = false;   // 앉기 토글 입력이 눌렸는지 여부.

    // Update is called once per frame
    void Update()
    {
        if(enableInput == false)
        {
            ClearInputState();
            return;
        }

        ReadMoveInput();
        ReadActionInput();
    }

    /// <summary>
    /// 이동 입력을 읽거 Vector2 형태로 저장.
    /// </summary>
    void ReadMoveInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    /// <summary>
    /// 점프, 질주, 앉기 입력을 읽는다.
    /// </summary>
    void ReadActionInput()
    {
        jumpPressed = Input.GetKeyDown(jumpKey);
        sprintHeld = Input.GetKey(sprintKey);   // 계속 누르고 있는지 체크.
        crouchTogglePressed = Input.GetKeyDown(crouchToggleKey);
    }

    /// <summary>
    /// 입력을 모두 초기화한다.
    /// </summary>
    void ClearInputState()
    {
        moveInput = Vector2.zero;
        jumpPressed = false;
        sprintHeld = false;
        crouchTogglePressed = false;
    }

    public void ConsumeJumpPressed()
    {
        jumpPressed = false;    // 점프 요청 상태가 적용이 끝났으면 false로 바꾼다.
    }

    public void ConsumeCrouchTogglePressed()
    {
        crouchTogglePressed = false;
    }

    public void SetInputEnable(bool enableInput)
    {
        this.enableInput = enableInput;
        if(this.enableInput == false)
        {
            ClearInputState();
        }
    }

    public Vector2 MoveInput
    {
        get
        {
            return moveInput;
        }
    }

    public bool JumpPressed
    {
        get { return jumpPressed; }
    }

    public bool SprintHeld
    {
        get { return sprintHeld; }
    }

    public bool CrouchTogglePressed
    {
        get { return crouchTogglePressed; }
    }

    public bool EnableInput
    {
        get { return enableInput; }
    }
}
