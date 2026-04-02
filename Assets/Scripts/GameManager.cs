using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
   
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
    [Header("Speed")]
    public Button increSpeedButton;
    public Button decreSpeedButton;
    public Scrollbar speedScrollbar;
    public TextMeshProUGUI speed_Text;
    private bool isUpdatingScrollbar = false;
    private float speed = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;

    public bool IsPaused { get; private set; } = false;
    private string lastJudgement = "";
    private int sameTypeCount = 0;
    private float snapThreshold = 0.03f; // khoảng hút (có thể chỉnh)
    private float[] snapPoints = new float[]
    {
    0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f
    };
    private Coroutine evaluateFadeRoutine;
    private Coroutine comboFadeRoutine;
    [Header("Start")]
    public float startCooldown = 1.0f;
    public bool IsGameStarted { get; private set; } = false;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartGameRoutine());

        evaluateText.text = "";
        comboText.text = "";
        comboCanvasGroup.alpha = 0;
        evaluateCanvasGroup.alpha = 0;

        speed = 1f;
        ApplySpeed();

        increSpeedButton.onClick.AddListener(IncreaseSpeed);
        decreSpeedButton.onClick.AddListener(DecreaseSpeed);
        speedScrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        UpdateUI();
    }
    void OnScrollbarChanged(float value)
    {
        if (isUpdatingScrollbar) return;

        float rawSpeed = Mathf.Lerp(0.5f, 2f, value);

        // 🎯 làm tròn 0.01 (mượt)
        float smoothSpeed = Mathf.Round(rawSpeed * 100f) / 100f;

        // 🎯 SNAP
        float snapped = GetSnappedSpeed(smoothSpeed);

        speed = snapped;

        ApplySpeed();
    }
    float GetSnappedSpeed(float input)
    {
        foreach (float point in snapPoints)
        {
            if (Mathf.Abs(input - point) <= snapThreshold)
            {
                return point; // 🎯 hút vào mốc
            }
        }

        return input; // 🎯 giữ mượt
    }
    public void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        StartCoroutine(AudioManager.Instance.FadeOutPitchThenStop(1f));
    }
    void IncreaseSpeed()
    {
        speed += 0.25f;
        speed = Mathf.Clamp(speed, 0.5f, 2f);

        ApplySpeed();
    }

    void DecreaseSpeed()
    {
        speed -= 0.25f;
        speed = Mathf.Clamp(speed, 0.5f, 2f);

        ApplySpeed();
    }
    void ApplySpeed()
    {
        SpeedMultiplier = speed;

        speed_Text.text = "x" + speed.ToString("0.##");

        float normalized = (speed - 0.5f) / (2f - 0.5f);

        // ❌ không dùng value =
        speedScrollbar.SetValueWithoutNotify(normalized);

        ChartManager.Instance.SetSpeed(SpeedMultiplier);
    }
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(startCooldown);

        AudioManager.Instance.PlaySong();

        IsGameStarted = true; // 🔥 MỐC DUY NHẤT bắt đầu game
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

        // UI
        pauseUI.SetActive(true);
        pauseButton.SetActive(false);

        // ⏸ Dừng nhạc
        AudioManager.Instance.PauseSong();

        // ⏸ Dừng TimeScale (freeze mọi Update)
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;

        // UI
        pauseUI.SetActive(false);
        pauseButton.SetActive(true);

        // ▶️ chạy lại nhạc (chuẩn DSP)
        AudioManager.Instance.ResumeSong();

        // ▶️ mở lại game
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

        // ===== COMBO =====
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

        // ===== UPDATE UI =====
        scoreText.text = score.ToString();

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