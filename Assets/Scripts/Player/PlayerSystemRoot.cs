using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player 내부 핵심 시스템 참조를 한곳에 모아서 관리하는 역할
/// 핵심 역할.
/// 1. Player 내부 주요 시스템 참조를 모은다.
/// 2. 다른 시스템이 Player 구성요소를 일관되게 참조할 수 있게 한다.
/// 3. 구조 점검용 Installer와 DebugUI가 쉽게 상태를 읽을 수 있게 한다.
/// </summary>
public class PlayerSystemRoot : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;

    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerLocomotionController locomotionController;
    [SerializeField] private PlayerStanceController stanceController;
    [SerializeField] private FpsLookController lookController;
    [SerializeField] private RaycastInteractor raycastInteractor;

    /// <summary>
    /// 현재 Player의 시스템 구조가 최소 조건을 만족하는지 여부를 반환.
    /// </summary>
    /// <returns>최소 조건 만족 여부</returns>
    public bool HasMinimumValidSetup()
    {
        bool hasCharacterController = characterController != null;
        bool hasPlayerCamera = playerCamera != null;
        bool hasCameraPivot = cameraPivot != null;
        bool hasInputReader = inputReader != null;
        bool hasLocomotionController = locomotionController != null;
        bool hasStanceController = stanceController != null;
        bool hasLookController = lookController != null;
        bool hasRaycastInteractor = raycastInteractor != null;

        bool hasMinimumSetup = hasCharacterController == true &&
            hasPlayerCamera == true && hasCameraPivot == true &&
            hasInputReader == true && hasLocomotionController == true &&
            hasStanceController == true && hasLookController == true &&
            hasRaycastInteractor == true;

        return hasMinimumSetup;
    }

    public CharacterController CharacterController
    {
        get { return characterController; }
    }

    public Camera PlayerCamera
    {
        get { return playerCamera; }
    }

    public Transform CameraPivot
    {
        get { return cameraPivot; }
    }

    public PlayerInputReader InputReader
    {
        get { return inputReader; }
    }

    public PlayerLocomotionController LocomotionController
    {
        get { return locomotionController; }
    }

    public PlayerStanceController StanceController
    {
        get { return stanceController; }
    }

    public FpsLookController LookController
    {
        get { return lookController; }
    }

    public RaycastInteractor RaycastInteractror
    {
        get { return raycastInteractor; }
    }
}
