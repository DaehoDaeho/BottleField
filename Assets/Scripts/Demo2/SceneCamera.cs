using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 실행 중 3D 씬을 자유롭게 관찰하기 위한 카메라 컨트롤 스크립트.
/// - 마우스 오른쪽 버튼을 누른 상태로 시점 회전
/// -WASD 이동
/// - E / Q 상승, 하강
/// -Shift 가속 이동
/// - 마우스 휠로 이동 속도 조절
/// </summary>
public class SceneCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float fastMoveMultiplier = 2.5f;   // 이동 속도 변화 비율.
    [SerializeField] private float verticalMoveSpeed = 6.0f;    // 상승/하강 시 속도.
    [SerializeField] private float minMoveSpeed = 2.0f;
    [SerializeField] private float maxMoveSpeed = 30.0f;
    [SerializeField] private float scrollSpeedStep = 1.0f;

    [SerializeField] private float lookSensitivity = 120.0f;    // 마우스 감도.
    [SerializeField] private float minPitch = -80.0f;
    [SerializeField] private float maxPitch = 80.0f;

    private float currentYaw = 0.0f;    // 현재의 좌우회전 값. (y축 회전)
    private float currentPitch = 0.0f;  // 현재의 상하회전값. (x축 회전)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 startEulerAngles = transform.rotation.eulerAngles;
        currentYaw = startEulerAngles.y;
        currentPitch = startEulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMoveSpeed();
        HandleLook();
        HandleMovement();
    }

    /// <summary>
    /// 마우스 휠 입력을 받아 이동 속도 조절.
    /// </summary>
    void UpdateMoveSpeed()
    {
        float scrollInput = Input.mouseScrollDelta.y;

        if(scrollInput != 0.0f)
        {
            moveSpeed += scrollInput * scrollSpeedStep;
            moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);
        }
    }

    /// <summary>
    /// 마우스 오른쪽 버튼을 누르고 있을 때 시점 회전을 처리.
    /// </summary>
    void HandleLook()
    {
        if(Input.GetMouseButton(1) == false)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentYaw += mouseX * lookSensitivity * Time.deltaTime;
        currentPitch -= mouseY * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0.0f);
    }

    /// <summary>
    /// WASD, E/Q 입력을 받아서 카메라를 이동.
    /// </summary>
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float forwardInput = Input.GetAxis("Vertical");

        float verticalInput = 0.0f;

        if(Input.GetKeyDown(KeyCode.E) == true)
        {
            verticalInput += 1.0f;
        }

        if(Input.GetKeyDown(KeyCode.Q) == true)
        {
            verticalInput -= 1.0f;
        }

        Vector3 moveDirection = ((transform.right * horizontalInput) + (transform.forward * forwardInput) +
            (Vector3.up * verticalInput)).normalized;

        float currentMoveSpeed = moveSpeed;
        if(Input.GetKeyDown(KeyCode.LeftShift) == true)
        {
            currentMoveSpeed *= fastMoveMultiplier;
        }

        Vector3 horizontalAndForwardMovement = ((transform.right * horizontalInput) + (transform.forward * forwardInput)) *
            currentMoveSpeed * Time.deltaTime;

        Vector3 verticalMovement = (Vector3.up * verticalInput) * verticalMoveSpeed * Time.deltaTime;

        transform.position += horizontalAndForwardMovement;
        transform.position += verticalMovement;
    }
}
