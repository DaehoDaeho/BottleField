using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 게임 클리어 시 클리어 카메라 연출을 재생하는 역할.
/// </summary>
public class ClearCameraSequenceController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera clearDollyCamera;
    [SerializeField] private CinemachineSplineDolly splineDolly;

    [SerializeField] private MonoBehaviour[] playerBehaviourToDisable;
    [SerializeField] private PlayerLocomotionController playerLocomotionController;

    // 카메라가 Spline의 시작점부터 끝점까지 이동하는 시간.
    [SerializeField] private float movementDuration = 7.0f;

    // 카메라 연출이 끝난 후 클리어 UI가 뜨기까지 대기할 시간.
    [SerializeField] private float endHoldDuration = 1.0f;

    // 시간의 진행률을 실제 카메라 이동 진행률로 변환할 곡선 변수.
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    // 현재 카메라 연출이 재생중인지 여부를 저장하는 변수.
    [SerializeField] private bool isPlaying = false;

    // 현재 카메라의 Spline 정규화 위치를 저장하는 변수.
    [SerializeField] private float currentNormalizedPosition = 0.0f;

    // 클리어 카메라 연출이 모두 끝났을 때 외부 시스템에 알리는 이벤트.
    public event Action SequenceFinished;

    public bool IsPlaying
    {
        get { return isPlaying; }
    }

    void SetPlayerBehaviourEnabled(bool enabled)
    {
        for(int i=0; i<playerBehaviourToDisable.Length; ++i)
        {
            playerBehaviourToDisable[i].enabled = enabled;
        }
    }

    void PreparePlayerForSequence()
    {
        if(playerLocomotionController != null)
        {
            playerLocomotionController.enabled = false;
        }

        SetPlayerBehaviourEnabled(false);
    }

    /// <summary>
    /// 클리어 카메라 연출의 전체 진행 순서를 처리.
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaySequenceCoroutine()
    {
        isPlaying = true;

        // 카메라의 spline 이동 위치를 시작점으로 초기화.
        currentNormalizedPosition = 0.0f;

        // spline의 위치 단위를 정규화 방식으로 설정.
        splineDolly.PositionUnits = PathIndexUnit.Normalized;

        // Clear Dolly Camera를 Spline 시작점에 배치.
        splineDolly.CameraPosition = currentNormalizedPosition;

        clearDollyCamera.gameObject.SetActive(true);

        yield return null;

        if(cinemachineBrain != null)
        {
            while(cinemachineBrain.IsBlending == true)
            {
                yield return null;
            }
        }

        float safeMovementDuration = Mathf.Max(0.01f, movementDuration);

        float elapsedTime = 0.0f;

        while(elapsedTime < safeMovementDuration)
        {
            elapsedTime += Time.deltaTime;

            // 전체 이동 시간 중 현재까지 진행한 비율을 계산.
            float timeRatio = elapsedTime / safeMovementDuration;

            timeRatio = Mathf.Clamp01(timeRatio);

            // AnimationCurve를 이용해서 감속과 감속을 포함한 이동 진행률을 계산.
            float curveValue = movementCurve.Evaluate(timeRatio);

            // Spline 위치가 0부터 1 사이를 벗어나지 않도록 제한.
            currentNormalizedPosition = Mathf.Clamp01(curveValue);

            // 현재 정규화 진행률을 Spline Dolly 카메라 위치에 적용.
            splineDolly.CameraPosition = currentNormalizedPosition;

            yield return null;
        }

        // 이동이 끝났으므로 현재 위치를 Spline 끝점으로 설정.
        currentNormalizedPosition = 1.0f;
        splineDolly.CameraPosition = currentNormalizedPosition;

        yield return new WaitForSeconds(safeMovementDuration);

        isPlaying = false;

        SequenceFinished.Invoke();
    }

    public bool PlaySequence()
    {
        if(isPlaying == true)
        {
            return false;
        }

        StartCoroutine(PlaySequenceCoroutine());
        return true;
    }

    public void PrepareInitialState()
    {
        currentNormalizedPosition = 0.0f;

        if(splineDolly != null)
        {
            splineDolly.PositionUnits = PathIndexUnit.Normalized;
            splineDolly.CameraPosition = currentNormalizedPosition;
        }

        if(clearDollyCamera != null)
        {
            clearDollyCamera.gameObject.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PrepareInitialState();
    }
}
