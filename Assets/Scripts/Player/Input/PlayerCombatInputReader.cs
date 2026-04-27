using UnityEngine;

/// <summary>
/// 발사 입력과 관련된 상태를 한곳에서 읽고 정리하는 역할.
/// 추후 조준, 재장전, 무기 교체 입력도 여기서 처리.
/// </summary>
public class PlayerCombatInputReader : MonoBehaviour
{
    // 현재 전투 입력을 허용할지 여부.
    [SerializeField] private bool enableCombatInput = true;

    // 발사 입력에 사용할 마우스 버튼 번호. 0:왼쪽 버튼, 1:오른쪽 버튼.
    [SerializeField] private int fireMouseButton = 0;

    // 이번 프레임에 발사 버튼이 눌렸는지 저장.
    private bool firePressed = false;

    // 현재 발사 버튼을 누르고 있는지 여부를 저장.
    private bool fireHeld = false;

    // Update is called once per frame
    void Update()
    {
        if(enableCombatInput == false)
        {
            ClearInputState();
            return;
        }

        firePressed = Input.GetMouseButtonDown(fireMouseButton);
        fireHeld = Input.GetMouseButton(fireMouseButton);
    }

    /// <summary>
    /// 현재 전투 입력 상태를 모두 초기화한다.
    /// </summary>
    void ClearInputState()
    {
        firePressed = false;
        fireHeld = false;
    }

    /// <summary>
    /// 전투 입력 상태 허용 여부를 변경.
    /// </summary>
    /// <param name="shouldEnableCombatInput">전투 입력 허용 여부</param>
    public void SetCombatInputEnabled(bool shouldEnableCombatInput)
    {
        enableCombatInput = shouldEnableCombatInput;
        if(enableCombatInput == false)
        {
            ClearInputState();
        }
    }

    public bool FirePressed
    {
        get { return firePressed; }
    }

    public bool FireHeld
    {
        get { return fireHeld; }
    }

    public bool EnableCombatInput
    {
        get { return enableCombatInput; }
        set
        {
            enableCombatInput = value;
            if (enableCombatInput == false)
            {
                ClearInputState();
            }
        }
    }
}
