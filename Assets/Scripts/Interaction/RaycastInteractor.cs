using UnityEngine;

/// <summary>
/// 카메라 기준으로 Raycast를 쏴서 현재 바라보고 있는 상호작용 대상을 찾고,
/// 입력이 들어오면 실제 상호작용을 수행하는 역할.
/// </summary>
public class RaycastInteractor : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private PlayerInputReader inputReader;

    [SerializeField] private float interactDistance = 4.0f;
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [SerializeField] private string currentTargetName;
    [SerializeField] private bool hasValidTarget = false;   // 현재 유효한 상호작용 대상이 있는지 여부.

    private IInteractable currentTarget = null;
    private RaycastHit currentHitInfo;
    private bool hasHit = false;

    private void Awake()
    {
        targetCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentTarget();
        HandleInteractInput();
    }

    /// <summary>
    /// 현재 카메라 정면으로 Raycast를 쏴서 상호작용 가능한 대상을 찾는다.
    /// 기존 대상과 새 대상을 비교해서 FocusEnter/FocusExit를 호출한다.
    /// </summary>
    void UpdateCurrentTarget()
    {
        if(targetCamera == null)
        {
            ClearCurrentTarget();
            return;
        }

        Transform cameraTransform = targetCamera.transform;
        Vector3 rayOrigin = cameraTransform.position;
        Vector3 rayDirection = cameraTransform.forward; // 카메라의 앞 방향.

        Ray interactionRay = new Ray(rayOrigin, rayDirection);  // 광선 생성.

        hasHit = Physics.Raycast(interactionRay, out currentHitInfo, interactDistance, interactableLayerMask);

        IInteractable nextTarget = null;
        if(hasHit == true)
        {
            nextTarget = currentHitInfo.collider.GetComponent<IInteractable>();
            if(nextTarget != null)
            {
                bool canInteract = nextTarget.CanInteract();
                if(canInteract == false)
                {
                    nextTarget = null;
                }
            }
        }

        if(currentTarget == nextTarget)
        {
            UpdateRuntimeDebugValue(nextTarget);
            return;
        }

        if(currentTarget != null)
        {
            currentTarget.OnFocusExit();
        }

        currentTarget = nextTarget;
        if (currentTarget != null)
        {
            currentTarget.OnFocusEnter();
        }

        UpdateRuntimeDebugValue(currentTarget);
    }

    /// <summary>
    /// 상호작용 입력이 들어왔을 때 현재 대상과 실제 상호작용을 수행.
    /// </summary>
    void HandleInteractInput()
    {
        if(currentTarget == null)
        {
            return;
        }

        if(inputReader != null && inputReader.EnableInput == true)
        {
            if(Input.GetKeyDown(interactKey) == true)
            {
                currentTarget.Interact();
            }
        }
    }

    /// <summary>
    /// 현재 대상을 완전히 해제한다.
    /// </summary>
    void ClearCurrentTarget()
    {
        if(currentTarget != null)
        {
            currentTarget.OnFocusExit();
        }

        currentTarget = null;
        currentTargetName = string.Empty;
        hasValidTarget = false;
        hasHit = false;
    }

    /// <summary>
    /// 현재 대상 기반으로 Inspector 값을 갱신한다.
    /// </summary>
    /// <param name="target"></param>
    void UpdateRuntimeDebugValue(IInteractable target)
    {
        hasValidTarget = target != null;

        if(target != null)
        {
            MonoBehaviour targetBehaviour = target as MonoBehaviour;
            if(targetBehaviour != null)
            {
                currentTargetName = targetBehaviour.name;
            }
            else
            {
                currentTargetName = string.Empty;
            }
        }
        else
        {
            currentTargetName = string.Empty;
        }
    }
}
