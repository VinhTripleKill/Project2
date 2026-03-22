using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private string lastJudgement = "";
    private int sameTypeCount = 0;

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

        UpdateUI();
    }
    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(startCooldown);

        AudioManager.Instance.PlaySong();

        IsGameStarted = true; // 🔥 MỐC DUY NHẤT bắt đầu game
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