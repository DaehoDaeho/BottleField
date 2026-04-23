using UnityEngine;

/// <summary>
/// 씬 시작 시 PlayerSystemRoot를 기준으로 전체 프로젝트 구조가 정상적으로 연결되어 있는지 확인하는 역할
/// </summary>
public class ProjectStructureInstaller : MonoBehaviour
{
    [SerializeField] private PlayerSystemRoot playerSystemRoot;

    // PlayerSystemRoot 참조가 비어 있을 때 자동 탐색할지 여부를 저장.
    [SerializeField] private bool autoFindPlayerSystemRoot = true;

    // 구조 점검 결과를 콘솔 창에 자세히 출력할지 여부를 저장.
    [SerializeField] private bool printDetailedLog = true;

    // 현재 프로젝트 구조가 유효한지 여부를 저장.
    [SerializeField] private bool isStructureValid = false;

    // 현재 구조 점점 요약 내용을 Inspector에서 확인하기 위한 변수.
    [SerializeField] private string validationSummary = string.Empty;

    private void Awake()
    {
        ValidataStructure();
    }

    /// <summary>
    /// 현재 프로젝트 구조의 핵심 참조가 올바른지 점검한다.
    /// </summary>
    public void ValidataStructure()
    {
        if(playerSystemRoot == null)
        {
            isStructureValid = false;
            validationSummary = "PlayerSystemRoot가 없다.";

            if(printDetailedLog == true)
            {
                Debug.LogWarning("프로젝트 구조 점검 실패 : PlayerSystemRoot를 찾을 수 없다.");
            }

            return;
        }

        bool hasMinimumSetup = playerSystemRoot.HasMinimumValidSetup();
        bool hasMoveReference = false;  // LocomotionController의 moveReference의 유효 여부를 저장할 변수.
        bool hasLookCameraPivot = false;    // CameraPivot의 유효 여부를 저장할 변수.
        bool hasRaycastCamera = false;  // RaycastInteractor의 카메라의 유효 여부를 저장할 변수.

        PlayerLocomotionController locomotionController = playerSystemRoot.LocomotionController;
        FpsLookController lookController = playerSystemRoot.LookController;
        RaycastInteractor raycastInteractor = playerSystemRoot.RaycastInteractror;
        Transform cameraPivot = playerSystemRoot.CameraPivot;
        Camera playerCamera = playerSystemRoot.PlayerCamera;

        if(locomotionController != null)
        {
            hasMoveReference = true;
        }

        if(lookController != null && cameraPivot != null)
        {
            hasLookCameraPivot = true;
        }

        if(raycastInteractor != null && playerCamera != null)
        {
            hasRaycastCamera = true;
        }

        isStructureValid = hasMinimumSetup == true && hasMoveReference == true &&
            hasLookCameraPivot == true && hasRaycastCamera == true;

        if(isStructureValid == true)
        {
            validationSummary = "점검 상태 양호";
        }
        else
        {
            validationSummary = "점검 상태 불량";
        }

        if (printDetailedLog == true)
        {
            Debug.Log("구조 점검 결과 -> " +
                "MinimumSetup : " + hasMinimumSetup + ", " +
                "MoveReference 준비상태: " + hasMoveReference + ", " +
                "CameraPivot 준비상태: " + hasLookCameraPivot + ", " +
                "RaycastCamera 준비상태: " + hasRaycastCamera + ", " +
                "Final: " + validationSummary);
        }
    }
}
