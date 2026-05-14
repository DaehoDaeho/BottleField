using UnityEngine;
using TMPro;

/// <summary>
/// 게임 결과 UI를 표시하는 역할.
/// </summary>
public class GameResultUi : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Text References")]
    [SerializeField] private TMP_Text gameOverTimeText;
    [SerializeField] private TMP_Text overMessageText;

    /// <summary>
    /// 모든 결과 UI 패널을 숨긴다.
    /// </summary>
    public void HidePanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 게임오버 UI 표시.
    /// </summary>
    /// <param name="survivalTime">생존 시간</param>
    public void ShowGameOver(float survivalTime, string message)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverTimeText != null)
        {
            gameOverTimeText.text = "Survival Time : " + Utility.FormatTime(survivalTime);
        }

        if(overMessageText != null)
        {
            overMessageText.text = message;
        }
    }
}