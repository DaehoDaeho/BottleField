using TMPro;
using UnityEngine;

public enum DemoGameState
{
    Ready,
    Playing,
    GameOver
}

/// <summary>
/// 전체 게임 흐름을 관리하는 매니저.
/// </summary>

public class GameFlowDemoManager : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text guideText;

    [SerializeField] private int targetScoreToClear = 30;   // 클리어를 위한 목표 스코어.

    private DemoGameState currentState = DemoGameState.Ready;
    private int currentScore = 0;

    // 프로퍼티 (Property) : 함수 대신에 외부에서 변수의 값을 참조할 수 있는 기능.
    // 여기서는 세팅 기능은 제공하지 않고, 값을 읽는 기능만 제공.
    public DemoGameState CurrentState
    {
        get { return currentState; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == DemoGameState.Ready)
        {
            if(Input.GetKeyDown(KeyCode.Space) == true)
            {
                StartDemoGame();
            }
        }

        if(currentState == DemoGameState.GameOver)
        {
            if(Input.GetKeyDown(KeyCode.R) == true)
            {
                ResetDemoGame();
            }
        }
    }

    public void StartDemoGame()
    {
        currentState = DemoGameState.Playing;
        UpdateUI();
    }

    public void AddScore(int addedScore)
    {
        if(currentState != DemoGameState.Playing)
        {
            return;
        }

        currentScore += addedScore;
        if(currentScore >= targetScoreToClear)
        {
            EndDemoGame();
        }

        UpdateUI();
    }

    public void EndDemoGame()
    {
        currentState = DemoGameState.GameOver;
        UpdateUI();
    }

    /// <summary>
    /// 게임 리셋.
    /// </summary>
    public void ResetDemoGame()
    {
        currentScore = 0;
        currentState = DemoGameState.Ready;
        UpdateUI();
    }

    void UpdateUI()
    {
        if(stateText != null)
        {
            stateText.text = "State : " + currentState.ToString();
        }

        if(scoreText != null)
        {
            scoreText.text = "Score : " + currentScore.ToString();
        }

        if(guideText != null)
        {
            if(currentState == DemoGameState.Ready)
            {
                guideText.text = "Press [Space] Key to Start Demo";
            }
            else if(currentState == DemoGameState.Playing)
            {
                guideText.text = "Press [Left Mouse] Button to Shoot";
            }
            else if(currentState == DemoGameState.GameOver)
            {
                guideText.text = "Press [R] Key to Reset Demo";
            }
        }
    }
}
