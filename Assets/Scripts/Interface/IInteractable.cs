using UnityEngine;

/// <summary>
/// 상호작용 가능한 대상이 공통으로 구현해야 할 기능을 정의하는 인터페이스.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 현재 오브젝트와 상호작용 가능한 상태로 들어왔을 때 호출.
    /// </summary>
    void OnFocusEnter();

    /// <summary>
    /// 현재 오브젝트가 상호작용 가능 상태에서 벗어났을 때 호출.
    /// </summary>
    void OnFocusExit();

    /// <summary>
    /// 실제 상호작용 입력이 들어왔을 때 호출.
    /// </summary>
    void Interact();

    bool CanInteract();
}
