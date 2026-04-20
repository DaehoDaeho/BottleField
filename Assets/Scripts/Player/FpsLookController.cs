using UnityEngine;

/// <summary>
/// FPS에서 가장 기본이 되는 시점 제어 구조를 담당
/// 1. 마우스 입력을 읽는다.
/// 2. 좌우 회전(Yaw)을 Player 오브젝트에 적용한다.
/// 3. 상하 회전(Pitch)을 CameraPivot에 적용한다.
/// 4. Pitch 회전 범위를 제한한다.
/// 5. 커서 잠금 상태를 관리한다.
/// </summary>
public class FpsLookController : MonoBehaviour
{
    // Pivot : 오브젝트의 중심위치.
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerMoveMotor moveMotor;

    [SerializeField] private float mouseSensitivity = 180.0f;    // 마우스 감도.
    [SerializeField] private float minPitch = -80.0f;
    [SerializeField] private float maxPitch = 80.0f;
    [SerializeField] private bool invertY = false;  // 마우스 Y 축 반전 여부.

    [SerializeField] private bool lockCursorOnStart = true;
    [SerializeField] private KeyCode unlockCursorKey = KeyCode.Escape;

    [SerializeField] private bool isCursorLocked = false;

    private float currentYaw = 0.0f;
    private float currentPitch = 0.0f;

    private void Awake()
    {
        Vector3 playerEulerAngles = transform.rotation.eulerAngles;
        currentYaw = playerEulerAngles.y;   // 플레이어의 Y 회전값을 현재 Yaw 초기값으로 저장.

        if(cameraPivot != null)
        {
            Vector3 pivotEulerAngles = cameraPivot.localRotation.eulerAngles;

            if(pivotEulerAngles.x > 180.0f)
            {
                // Unity 오일러 각이 0~360으로 표현될 수 있으므로 180을 초과하는 값은 음수로 변환.
                pivotEulerAngles.x -= 360.0f;
            }

            // 카메라 피봇의 x 값을 현재 Pitch 초기값으로 저장.
            currentPitch = pivotEulerAngles.x;
        }

        SetCursorLock(lockCursorOnStart);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCursorState();
        UpdateLookRotation();
    }

    void SetCursorLock(bool cursorLock)
    {
        isCursorLocked = cursorLock;

        if(cursorLock == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// ESC 키로 커서 잠금을 해제하고, 마우스 왼쪽 버튼 클릭으로 다시 잠그는 처리.
    /// </summary>
    void UpdateCursorState()
    {
        if(Input.GetKeyDown(unlockCursorKey) == true)
        {
            SetCursorLock(false);
        }

        if(isCursorLocked == false)
        {
            if(Input.GetMouseButtonDown(0) == true)
            {
                SetCursorLock(true);
            }
        }
    }

    /// <summary>
    /// 마우스 입력을 읽어서 Yaw와 Pitch를 누적하고, Player와 CameraPivot에 적용한다.
    /// </summary>
    void UpdateLookRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float yawDelta = mouseX * mouseSensitivity * Time.deltaTime;    // yaw 변화량 계산.
        float rawPitchDelta = mouseY * mouseSensitivity * Time.deltaTime;
        float pitchDelta = invertY == true ? rawPitchDelta : -rawPitchDelta;

        currentYaw += yawDelta;
        currentPitch += pitchDelta;

        // Mathf.Clamp 함수로 최소/최대 범위 적용.
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        Quaternion playerYawRotation = Quaternion.Euler(0.0f, currentYaw, 0.0f);
        Quaternion cameraPitchRotation = Quaternion.Euler(currentPitch, 0.0f, 0.0f);

        transform.rotation = playerYawRotation;
        cameraPivot.localRotation = cameraPitchRotation;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 playerOrigin = transform.position;
        Vector3 playerForwardEnd = playerOrigin + (transform.forward * 2.0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerOrigin, playerForwardEnd);

        Vector3 cameraOrigin = cameraPivot.position;
        Vector3 cameraForwardEnd = cameraOrigin + (cameraPivot.forward * 2.0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(cameraOrigin, cameraForwardEnd);
    }
}
