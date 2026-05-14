using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Clear
}

/// <summary>
/// 전체 게임 흐름을 관리하는 매니저 스크립트.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth; // 플레이어 생존 여부와 체력 정보를 읽기 위한 참조.
    [SerializeField] private GameResultUi resultUi; // 게임오버와 클리어 화면을 표시하기 위한 UI 참조.

    [Header("Clear Settings")]
    [SerializeField] private float clearSurvivalTime = 180.0f; // 클리어에 필요한 목표 생존 시간을 저장.

    [Header("Runtime")]
    [SerializeField] private GameState currentState = GameState.Playing; // 현재 게임 상태를 저장.
    [SerializeField] private float survivalTimer = 0.0f; // 현재까지 플레이어가 생존한 시간을 저장.
    [SerializeField] private float remainTimer = 0.0f;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 게임 시작 시 게임 상태와 시간 흐름을 초기화한다.
    /// </summary>
    private void Start()
    {
        currentState = GameState.Playing;
        survivalTimer = 0.0f;
        remainTimer = clearSurvivalTime;

        Time.timeScale = 1.0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (resultUi != null)
        {
            resultUi.HidePanel();
        }
    }

    /// <summary>
    /// 매 프레임 게임 상태를 검사하고 Playing 상태일 때만 진행 로직을 처리한다.
    /// </summary>
    private void Update()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        CheckPlayerDeath();
        UpdateSurvivalTimer();
        CheckClearCondition();
    }

    /// <summary>
    /// 플레이어가 사망했는지 확인하고 사망했다면 게임오버 처리한다.
    /// </summary>
    private void CheckPlayerDeath()
    {
        if (playerHealth == null)
        {
            return;
        }

        if (playerHealth.IsDead == true)
        {
            SetGameOver();
        }
    }

    /// <summary>
    /// 플레이어 생존 시간을 증가시킨다.
    /// </summary>
    private void UpdateSurvivalTimer()
    {
        survivalTimer += Time.deltaTime;
        remainTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 생존 시간이 목표 시간에 도달했는지 확인하고 도달했다면 클리어 처리한다.
    /// </summary>
    private void CheckClearCondition()
    {
        if (survivalTimer >= clearSurvivalTime)
        {
            SetClear();
        }
    }

    /// <summary>
    /// 게임을 게임오버 상태로 전환한다.
    /// </summary>
    private void SetGameOver()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.GameOver;
        StopGameTime();
        ShowCursor();

        if (resultUi != null)
        {
            resultUi.ShowGameOver(survivalTimer, "Game Over");
        }
    }

    /// <summary>
    /// 게임을 클리어 상태로 전환한다.
    /// </summary>
    private void SetClear()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.Clear;
        StopGameTime();
        ShowCursor();

        if (resultUi != null)
        {
            resultUi.ShowGameOver(survivalTimer, "Game Clear");
        }
    }

    /// <summary>
    /// 게임 시간 흐름을 멈춘다.
    /// </summary>
    private void StopGameTime()
    {
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// 마우스 커서를 UI 조작 가능 상태로 표시한다.
    /// </summary>
    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 현재 씬을 다시 불러와 게임을 재시작한다.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1.0f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// 게임을 종료한다.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 현재 게임 상태를 외부에서 읽을 수 있도록 제공한다.
    /// </summary>
    public GameState CurrentState
    {
        get { return currentState; }
    }

    /// <summary>
    /// 현재 생존 시간을 외부에서 읽을 수 있도록 제공한다.
    /// </summary>
    public float SurvivalTimer
    {
        get { return survivalTimer; }
    }

    /// <summary>
    /// 현재 남은 시간을 외부에서 읽을 수 있도록 제공한다.
    /// </summary>
    public float RemainTimer
    {
        get { return remainTimer; }
    }
}