using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    public static GameManager Instance;
    public float FinalPlayTime { get; private set; }
    [Header("Result")]
    public GameObject resultGame;
    [Header("Result Setting")]
    [SerializeField] private float cooldownByLose = 2f;
    [Header("UI")]
    public TextMeshProUGUI evaluateText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;

    public CanvasGroup evaluateCanvasGroup;
    public CanvasGroup comboCanvasGroup;

    public float comboDuration = 0.5f;
    public float evaluateDuration = 0.5f;

    private int score = 0;
    private int combo = 0;

    [Header("Pause")]
    public GameObject pauseUI;
    public GameObject pauseButton;

    public bool IsGameOver { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;

    private string lastJudgement = "";
    private int sameTypeCount = 0;

    private Coroutine evaluateFadeRoutine;
    private Coroutine comboFadeRoutine;

    [Header("Start")]
    public float startCooldown = 1.0f;
    public bool IsGameStarted { get; private set; } = false;

    // Speed
    private float speed = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Khởi tạo UI
        evaluateText.text = "";
        comboText.text = "";
        comboCanvasGroup.alpha = 0;
        evaluateCanvasGroup.alpha = 0;
        score = 0;
        // Khởi tạo tốc độ mặc định
        speed = 1f;
        SetSpeed(1f);

        StartCoroutine(StartGameRoutine());
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Clamp(newSpeed, 0.5f, 2f);
        SpeedMultiplier = speed;

        if (ChartManager.Instance != null)
            ChartManager.Instance.SetSpeed(SpeedMultiplier);
    }

    IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        resultGame.SetActive(true);
    }
    public int GetScore()
    {
        return score;
    }
    public void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        SaveDataManager.SaveStars(progressBar.GetCurrentStars());
        SaveDataManager.SaveScore(score);

        // ⏱ lưu thời gian ngay lúc chết
        FinalPlayTime = (float)AudioManager.Instance.SongTimeDSP;

        StartCoroutine(AudioManager.Instance.FadeOutPitchThenStop(1f));
        StartCoroutine(ShowResultAfterDelay(cooldownByLose));
    }
    bool HasActiveNotes()
    {
        return ObjectPoolingManager.Instance.activeNoteCount > 0;
        
    }
    public void OnSongFinished()
    {
        SaveDataManager.SaveStars(progressBar.GetCurrentStars());
        FinalPlayTime = AudioManager.Instance.audioSource.clip.length;

        StartCoroutine(WaitForAllNotesThenShowResult());
    }
    IEnumerator WaitForAllNotesThenShowResult()
    {
        // chờ cho tới khi hết note
        while (HasActiveNotes())
        {
            yield return null;
        }

        // 🔥 LÚC NÀY mới tính điểm & lưu
        SaveDataManager.SaveStars(progressBar.GetCurrentStars());
        SaveDataManager.SaveScore(score);

        FinalPlayTime = AudioManager.Instance.audioSource.clip.length;

        resultGame.SetActive(true);
    }
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(startCooldown);

        AudioManager.Instance.PlaySong();
        IsGameStarted = true;
    }

    public void ToggleStatus()
    {
        if (IsPaused)
            Resume();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;

        pauseUI.SetActive(true);
        pauseButton.SetActive(false);

        // Set default tab khi mở Pause
        PauseUI.Instance.InitDefault();

        AudioManager.Instance.PauseSong();
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;

        pauseUI.SetActive(false);
        pauseButton.SetActive(true);

        AudioManager.Instance.ResumeSong();
        Time.timeScale = 1f;
    }

    public void ProcessJudgement(string judgement)
    {
        int addScore = 0;

        switch (judgement)
        {
            case "PERFECT": addScore = 5; break;
            case "GREAT": addScore = 3; break;
            case "GOOD": addScore = 2; break;
            case "MISS":
                addScore = 0;
                HpBar.Instance.TakeMissDamage();
                break;
        }

        score += addScore;

        // Combo logic
        if (judgement == "MISS")
        {
            combo = 0;
            sameTypeCount = 0;
            lastJudgement = "";
        }
        else
        {
            if (judgement == lastJudgement)
            {
                sameTypeCount++;
                if (sameTypeCount >= 2)
                    combo++;
            }
            else
            {
                lastJudgement = judgement;
                sameTypeCount = 1;
                combo = 0;
            }
        }
        UpdateUI();
        ShowEvaluate(judgement);
        ShowCombo();
    }

    void ShowEvaluate(string judgement)
    {
        evaluateText.text = judgement;
        evaluateCanvasGroup.alpha = 1;

        if (evaluateFadeRoutine != null)
            StopCoroutine(evaluateFadeRoutine);

        evaluateFadeRoutine = StartCoroutine(FadeOut(evaluateCanvasGroup, evaluateDuration));
    }

    void ShowCombo()
    {
        if (combo <= 0)
        {
            comboCanvasGroup.alpha = 0;
            return;
        }

        comboText.text = "x" + combo.ToString();
        comboCanvasGroup.alpha = 1;

        if (comboFadeRoutine != null)
            StopCoroutine(comboFadeRoutine);

        comboFadeRoutine = StartCoroutine(FadeOut(comboCanvasGroup, comboDuration));
    }


    public int GetCurrentRunStars()
    {
        return progressBar.GetCurrentRunStarCount();
    }
    IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        yield return new WaitForSeconds(duration);

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 0;
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
    }
}