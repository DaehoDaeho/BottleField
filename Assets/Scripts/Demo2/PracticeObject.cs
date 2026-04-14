using UnityEngine;

/// <summary>
/// 3D 좌표, 로컬/월드 이동, 회전을 직접 확인할 수 있는 오브젝트 제어 스크립트
/// -방향키로 오브젝트 이동
/// -U / O 키로 회전
/// - 1 키로 월드 이동/로컬 이동 모드 전환
/// - 2 키로 자동 회전 on/off
/// </summary>
public class PracticeObject : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float rotationSpeed = 90.0f;

    [SerializeField] private bool useLocalSpaceMovement = true;
    [SerializeField] private bool autoRotation = false;
    [SerializeField] private float autoRotationSpeed = 45.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleModeToggle();
        HandleManualRotation();
        HandleMovement();
        HandleAutoRotation();
    }

    void HandleModeToggle()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            useLocalSpaceMovement = !useLocalSpaceMovement;
        }

        if(Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            autoRotation = !autoRotation;
        }
    }

    void HandleManualRotation()
    {
        float rotationInput = 0.0f;
        if(Input.GetKeyDown(KeyCode.U) == true)
        {
            rotationInput -= 1.0f;
        }

        if (Input.GetKeyDown(KeyCode.O) == true)
        {
            rotationInput += 1.0f;
        }

        if(rotationInput != 0.0f)
        {
            float yRotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0.0f, yRotationAmount, 0.0f, Space.Self);
        }
    }

    void HandleMovement()
    {
        float moveX = 0.0f;
        float moveZ = 0.0f;

        if(Input.GetKey(KeyCode.LeftArrow) == true)
        {
            moveX -= 1.0f;
        }

        if (Input.GetKey(KeyCode.RightArrow) == true)
        {
            moveX += 1.0f;
        }

        if (Input.GetKey(KeyCode.UpArrow) == true)
        {
            moveZ += 1.0f;
        }

        if (Input.GetKey(KeyCode.DownArrow) == true)
        {
            moveZ -= 1.0f;
        }

        Vector3 moveInput = new Vector3(moveX, 0.0f, moveZ).normalized;

        Vector3 moveDelta = moveInput * moveSpeed * Time.deltaTime;

        if(useLocalSpaceMovement == true)
        {
            transform.Translate(moveDelta, Space.Self);
        }
        else
        {
            transform.position += moveDelta;
        }
    }

    void HandleAutoRotation()
    {
        if(autoRotation == false)
        {
            return;
        }

        float autoRotateAmount = autoRotationSpeed * Time.deltaTime;
        transform.Rotate(0.0f, autoRotateAmount, 0.0f, Space.Self);
    }
}
