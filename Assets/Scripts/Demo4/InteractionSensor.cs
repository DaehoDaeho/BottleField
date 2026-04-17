using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어 주변 Trigger 범위 안에 들어온 상호작용 가능한 대상을 탐지.
/// </summary>
public class InteractionSensor : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private KeyCode interacKey = KeyCode.E;

    [SerializeField] private string currentTargetName = string.Empty;
    [SerializeField] private int candidateCount = 0;

    private List<IInteractable> interactableCandidates = new List<IInteractable>();
    private IInteractable currentInteractable = null;

    // Update is called once per frame
    void Update()
    {
        RefreshCurrentTarget();

        if(currentInteractable != null)
        {
            if(Input.GetKeyDown(interacKey) == true)
            {
                currentInteractable.Interact();
            }
        }
    }

    /// <summary>
    /// 인자로 전달받은 레이어가 LayerMask에 포함되어 있는지 검사한다.
    /// </summary>
    /// <param name="layer">검사할 대상 오브젝트의 레이어</param>
    /// <param name="layerMask">비교할 LayerMask 값</param>
    /// <returns></returns>
    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        int layerBit = 1 << layer;  // 레이어 번호를 비트 마스크 값으로 변환하는 계산.
        int maskedValue = layerMask.value & layerBit; // LayerMask와 해당 layerBit AND 연산으로 비교.
        return maskedValue != 0;    // 0이면 미포함, 0이 아니면 포함
    }

    /// <summary>
    /// 파괴됐거나 null이 된 대상은 목록에서 제외한다.
    /// </summary>
    void RemoveNullCandidates()
    {
        // 리스트에서 목록을 여러개 지울 때는 역순으로 돌아야 한다.
        for(int i=interactableCandidates.Count-1; i>=0; --i)
        {
            if (interactableCandidates[i] == null)
            {
                interactableCandidates.RemoveAt(i);
            }
        }

        candidateCount = interactableCandidates.Count;
    }

    /// <summary>
    /// 현재 후보들 중 가장 가까운 대상을 선택한다.
    /// </summary>
    void RefreshCurrentTarget()
    {
        RemoveNullCandidates();

        IInteractable nearestInteractable = null;
        float nearestDistanceSqr = float.MaxValue;  // 가장 가까운 거리값을 비교하기 위한 초기화.
        Vector3 sensorPosition = transform.position;

        for(int i=0; i<interactableCandidates.Count; ++i)
        {
            IInteractable candidate = interactableCandidates[i];

            if(candidate == null)
            {
                continue;
            }

            MonoBehaviour candidateBehaviour = candidate as MonoBehaviour;

            Vector3 candidatePosition = candidateBehaviour.transform.position;
            Vector3 toCandidate = candidatePosition - sensorPosition;
            float distanceSqr = toCandidate.sqrMagnitude;

            if(distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestInteractable = candidate;
            }
        }

        if(currentInteractable == nearestInteractable)
        {
            if(currentInteractable != null)
            {
                MonoBehaviour currentBehaviour = currentInteractable as MonoBehaviour;
                currentTargetName = currentBehaviour.name;
            }
            else
            {
                currentTargetName = string.Empty;
            }

            return;
        }

        if(currentInteractable != null)
        {
            currentInteractable.OnFocusExit();
        }

        currentInteractable = nearestInteractable;

        if(currentInteractable != null)
        {
            currentInteractable.OnFocusEnter();

            MonoBehaviour currentBehaviour = currentInteractable as MonoBehaviour;
            currentTargetName = currentBehaviour.name;
        }
        else
        {
            currentTargetName = string.Empty;
        }
    }

    private void OnTriggerEnter(Collider other)    
    {
        if(IsInLayerMask(other.gameObject.layer, interactableLayerMask) == false)
        {
            return;
        }

        IInteractable interactable = other.GetComponent<IInteractable>();

        if(interactable == null)
        {
            return;
        }

        if(interactableCandidates.Contains(interactable) == true)
        {
            return;
        }

        interactableCandidates.Add(interactable);
        candidateCount = interactableCandidates.Count;

        RefreshCurrentTarget();
    }

    void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return;
        }

        if (interactableCandidates.Contains(interactable) == false)
        {
            return;
        }

        bool wasCurrentTarget = interactable == currentInteractable;

        if(wasCurrentTarget == true)
        {
            currentInteractable.OnFocusExit();
            currentInteractable = null;
            currentTargetName = string.Empty;
        }

        RefreshCurrentTarget();
    }
}
