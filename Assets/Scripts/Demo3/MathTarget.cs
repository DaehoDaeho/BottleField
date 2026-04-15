using UnityEngine;

/// <summary>
/// 목표 지점 역할을 담당.
/// </summary>

public class MathTarget : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float heightStep = 1.0f;
        
    // Update is called once per frame
    void Update()
    {
        float moveX = 0.0f;
        float moveY = 0.0f;
        float moveZ = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow) == true)
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

        if (Input.GetKeyDown(KeyCode.E) == true)
        {
            moveY += heightStep;
        }

        if (Input.GetKeyDown(KeyCode.Q) == true)
        {
            moveY -= heightStep;
        }

        Vector3 moveInput = new Vector3(moveX, 0.0f, moveZ).normalized;

        Vector3 moveDelta = moveInput * moveSpeed * Time.deltaTime;
        Vector3 verticalMoveDelta = new Vector3(0.0f, moveY, 0.0f);
        Vector3 finalMoveDelta = moveDelta + verticalMoveDelta;

        transform.position += finalMoveDelta;
    }
}
