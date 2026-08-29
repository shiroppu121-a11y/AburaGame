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
    private GameObject timeUpUI;

    [Header("画面")]

    [SerializeField]
    private GameObject stageClearUI;

    [SerializeField]
    private GameObject introPanel;

    [SerializeField]
    private GameObject hintPanel;

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
    private float sceneChangeDelay = 2.0f;

    private float remainingTime;

    private bool isGameStarted;
    private bool isStageCleared;
    private bool isTimeUp;
    private bool isLoadingNextStage;

    private void Start()
    {
        isGameStarted = false;
        isStageCleared = false;
        isTimeUp = false;
        isLoadingNextStage = false;

        remainingTime = timeLimit;

        if (stageClearUI != null)
        {
            stageClearUI.SetActive(false);
        }

        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }

        if (timeUpUI != null)
        {
            timeUpUI.SetActive(false);
        }

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = timeLimit;
            timeSlider.value = remainingTime;
            timeSlider.interactable = false;
        }

        UpdateTimerDisplay();
        CheckArrayLength();
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

        isGameStarted = true;

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }

        if (soundManager != null)
        {
            soundManager.PlayGameStartSE();
            soundManager.PlayBGM();
        }

        Debug.Log("Game Start!");
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);

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

        UpdateTimerDisplay();
        SetCupOperationEnabled(false);

        if (timeUpUI != null)
        {
            timeUpUI.SetActive(true);
        }

        if (soundManager != null)
        {
            soundManager.StopBGM();
            soundManager.StopSE();
        }

        Debug.Log("Time Up!");
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

        StageClear();
    }

    private void StageClear()
    {
        if (isStageCleared)
        {
            return;
        }

        isStageCleared = true;

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

        Debug.Log("Stage Clear!");
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                "CupsまたはTarget Liquidsが設定されていません。"
            );

            return;
        }

        if (cups.Length != targetLiquids.Length)
        {
            Debug.LogError(
                "CupsとTarget Liquidsの要素数を合わせてください。"
            );
        }
    }

    public void ShowHint()
    {
        hintPanel.SetActive(true);
        soundManager.PlayHintSE();
    }

    public void CloseHint()
    {
        hintPanel.SetActive(false);
    }
}