using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [Header("制限時間")]

    [SerializeField]
    private float timeLimit = 60f;

    [SerializeField]
    private Slider timeSlider;

    [SerializeField]
    private TMP_Text remainingTimeText;

    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private GameObject timeUpUI;

    [Header("画面")]

    [SerializeField]
    private GameObject stageClearUI;

    [SerializeField]
    private GameObject introPanel;

    [SerializeField]
    private GameObject hintPanel;

    [SerializeField]
    private GameObject hintButton;

    [SerializeField]
    private GameObject allClearPanel;

    [Header("音声")]

    [SerializeField]
    private SoundManager soundManager;

    [Header("ステージクリア条件")]

    [SerializeField]
    private Cup[] cups;

    [SerializeField]
    private float[] targetLiquids;

    [SerializeField]
    private float tolerance = 0.01f;

    [Header("次のステージ")]

    [SerializeField]
    private string nextSceneName;

    [SerializeField]
    private float sceneChangeDelay = 0.2f;

    [Header("開始画面を表示するシーン")]

    [SerializeField]
    private string introSceneName = "FirstStage";

    private float remainingTime;

    private bool isGameStarted;
    private bool isStageCleared;
    private bool isTimeUp;
    private bool isLoadingNextStage;
    private bool isCountRunning;

    private double stageStartTime;

    /*
     * staticなのでシーンを切り替えても値が維持される。
     */
    private static double totalElapsedTime;
    private static bool hasRunStarted;

    private void Start()
    {
        isGameStarted = false;
        isStageCleared = false;
        isTimeUp = false;
        isLoadingNextStage = false;
        isCountRunning = false;

        InitializePanels();
        InitializeTimer();
        CheckArrayLength();

        bool shouldShowIntro =
            SceneManager.GetActiveScene().name ==
            introSceneName;

        if (introPanel != null)
        {
            introPanel.SetActive(shouldShowIntro);
        }

        /*
         * 最初のステージ以外では、
         * シーン開始と同時に計測を再開する。
         */
        if (!shouldShowIntro)
        {
            StartGame();
        }
    }

    private void InitializePanels()
    {
        if (stageClearUI != null)
        {
            stageClearUI.SetActive(false);
        }

        if (timeUpUI != null)
        {
            timeUpUI.SetActive(false);
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        if (allClearPanel != null)
        {
            allClearPanel.SetActive(false);
        }

        if (hintButton != null)
        {
            hintButton.SetActive(true);
        }
    }

    private void InitializeTimer()
    {
        timeLimit = Mathf.Max(timeLimit, 0f);
        remainingTime = timeLimit;

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = timeLimit;
            timeSlider.value = remainingTime;
            timeSlider.interactable = false;
        }

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            CheckStartInput();
            return;
        }

        if (isStageCleared || isTimeUp)
        {
            return;
        }

        UpdateTimer();
        CheckStageClearCondition();
    }

    private void CheckStartInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (isGameStarted)
        {
            return;
        }

        bool isFirstStage =
            SceneManager.GetActiveScene().name ==
            introSceneName;

        /*
         * ゲーム全体を最初に開始したときだけ、
         * 合計時間を0へ戻す。
         *
         * リスタートした場合はhasRunStartedがtrueなので、
         * それまでの時間は維持される。
         */
        if (isFirstStage && !hasRunStarted)
        {
            totalElapsedTime = 0.0;
            hasRunStarted = true;
        }

        isGameStarted = true;

        StartTimeCount();
        SetCupOperationEnabled(true);

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }

        if (soundManager != null)
        {
            soundManager.PlayGameStartSE();
            soundManager.PlayBGM();
        }

        Debug.Log(
            "Game Start! 現在の合計時間: " +
            totalElapsedTime.ToString("F2") +
            "秒"
        );
    }

    private void StartTimeCount()
    {
        if (isCountRunning)
        {
            return;
        }

        isCountRunning = true;

        stageStartTime =
            Time.realtimeSinceStartupAsDouble;
    }

    private void StopTimeCount()
    {
        if (!isCountRunning)
        {
            return;
        }

        double currentTime =
            Time.realtimeSinceStartupAsDouble;

        double stageElapsedTime =
            currentTime - stageStartTime;

        totalElapsedTime += stageElapsedTime;
        isCountRunning = false;

        Debug.Log(
            "今回の経過時間: " +
            stageElapsedTime.ToString("F2") +
            "秒 / 合計: " +
            totalElapsedTime.ToString("F2") +
            "秒"
        );
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        remainingTime = Mathf.Max(
            remainingTime,
            0f
        );

        UpdateTimerDisplay();

        if (remainingTime <= 0f)
        {
            TimeUp();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timeSlider != null)
        {
            timeSlider.value = remainingTime;
        }

        if (remainingTimeText != null)
        {
            int displaySeconds =
                Mathf.CeilToInt(remainingTime);

            remainingTimeText.text =
                displaySeconds.ToString();
        }
    }

    private void TimeUp()
    {
        if (isTimeUp)
        {
            return;
        }

        isTimeUp = true;
        remainingTime = 0f;

        /*
         * 時間切れになるまでの時間も、
         * 合計クリアタイムへ加算する。
         */
        StopTimeCount();

        UpdateTimerDisplay();
        SetCupOperationEnabled(false);
        HideHint();

        if (timeUpUI != null)
        {
            timeUpUI.SetActive(true);
        }

        if (soundManager != null)
        {
            soundManager.StopBGM();
            soundManager.StopSE();
        }

        Debug.Log(
            "Time Up! 合計時間: " +
            totalElapsedTime.ToString("F2") +
            "秒"
        );
    }

    private void CheckStageClearCondition()
    {
        if (cups == null || targetLiquids == null)
        {
            return;
        }

        if (cups.Length == 0 ||
            cups.Length != targetLiquids.Length)
        {
            return;
        }

        for (int i = 0; i < cups.Length; i++)
        {
            if (cups[i] == null)
            {
                return;
            }

            float difference = Mathf.Abs(
                cups[i].CurrentLiters -
                targetLiquids[i]
            );

            if (difference > tolerance)
            {
                return;
            }
        }

        HideHint();

        if (string.IsNullOrEmpty(nextSceneName))
        {
            AllClear();
        }
        else
        {
            StageClear();
        }
    }

    private void StageClear()
    {
        if (isStageCleared)
        {
            return;
        }

        isStageCleared = true;

        /*
         * このステージで操作していた時間を
         * 合計へ追加する。
         */
        StopTimeCount();
        SetCupOperationEnabled(false);

        if (stageClearUI != null)
        {
            stageClearUI.SetActive(true);
        }

        if (soundManager != null)
        {
            soundManager.StopBGM();
            soundManager.StopSE();
            soundManager.PlayStageClearSE();
        }

        Debug.Log(
            "Stage Clear! 合計時間: " +
            totalElapsedTime.ToString("F2") +
            "秒"
        );
    }

    private void AllClear()
    {
        if (isStageCleared)
        {
            return;
        }

        isStageCleared = true;

        /*
         * 最後のステージで操作していた時間を
         * 合計へ追加する。
         */
        StopTimeCount();
        SetCupOperationEnabled(false);

        if (allClearPanel != null)
        {
            allClearPanel.SetActive(true);
        }

        if (timeText != null)
        {
            timeText.text =
                "Time: " +
                FormatElapsedTime(totalElapsedTime);
        }

        if (soundManager != null)
        {
            soundManager.StopBGM();
            soundManager.StopSE();
            soundManager.PlayAllClearSE();
        }

        Debug.Log(
            "All Clear! 合計時間: " +
            totalElapsedTime.ToString("F2") +
            "秒"
        );
    }

    private string FormatElapsedTime(
        double elapsedTime
    )
    {
        int minutes =
            Mathf.FloorToInt(
                (float)elapsedTime / 60f
            );

        double seconds =
            elapsedTime % 60.0;

        return string.Format(
            "{0:00}:{1:00.00}",
            minutes,
            seconds
        );
    }

    public void LoadNextStage()
    {
        if (!isStageCleared)
        {
            return;
        }

        if (isLoadingNextStage)
        {
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError(
                "次のシーン名が設定されていません"
            );

            return;
        }

        isLoadingNextStage = true;

        if (soundManager != null)
        {
            soundManager.PlayGameStartSE();
        }

        StartCoroutine(
            LoadNextSceneAfterDelay()
        );
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(
            sceneChangeDelay
        );

        SceneManager.LoadScene(nextSceneName);
    }

    public void Restart()
    {
        /*
         * リスタートを押すまでの時間を
         * 合計へ追加してから再読み込みする。
         *
         * TimeUpで既に停止済みの場合は、
         * StopTimeCount内の判定によって
         * 二重加算されない。
         */
        StopTimeCount();

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(
            currentSceneName
        );
    }

    public void BackTitle()
    {
        StopTimeCount();

        /*
         * タイトルへ戻る場合は、
         * 現在の挑戦を終了してタイムをリセットする。
         */
        totalElapsedTime = 0.0;
        hasRunStarted = false;

        SceneManager.LoadScene(
            introSceneName
        );
    }

    private void SetCupOperationEnabled(
        bool isEnabled
    )
    {
        if (cups == null)
        {
            return;
        }

        foreach (Cup cup in cups)
        {
            if (cup != null)
            {
                cup.CanDrag = isEnabled;
            }
        }
    }

    private void CheckArrayLength()
    {
        if (cups == null || targetLiquids == null)
        {
            Debug.LogError(
                "CupsまたはTarget Liquidsが" +
                "設定されていません。"
            );

            return;
        }

        if (cups.Length != targetLiquids.Length)
        {
            Debug.LogError(
                "CupsとTarget Liquidsの" +
                "要素数を合わせてください。"
            );
        }
    }

    public void ShowHint()
    {
        if (isStageCleared || isTimeUp)
        {
            return;
        }

        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
        }

        if (soundManager != null)
        {
            soundManager.PlayHintSE();
        }
    }

    public void CloseHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    private void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        if (hintButton != null)
        {
            hintButton.SetActive(false);
        }
    }
}