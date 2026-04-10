using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    public Button replayButton;
    [Header("Rank System")]
    [SerializeField] private List<float> rankThresholds; // % (0 → 100)
    [SerializeField] private List<Sprite> rankSprites;

    [SerializeField] private Image rankResultImage;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI total_stars_collected;
    [SerializeField] private TextMeshProUGUI durationTimePlay;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI highestScore;
    [Header("Evaluate Stats")]
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI goodText;
    [SerializeField] private TextMeshProUGUI missText;

    [SerializeField] private TextMeshProUGUI maxCombo;
    void Start()
    {
        replayButton.onClick.AddListener(OnReplay);
        UpdateResultInfo();
    }

    void UpdateResultInfo()
    {
        if (GameManager.Instance == null) return;

        // ===== ⭐ STAR =====
        int stars = GameManager.Instance.GetCurrentRunStars();
        total_stars_collected.text = $"Stars: {stars}";

        // ===== ⏱ TIME =====
        float time = GameManager.Instance.FinalPlayTime;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        durationTimePlay.text = $"Time: {minutes:00}:{seconds:00}";

        // ===== 🎯 SCORE HIỆN TẠI =====
        int currentScore = GameManager.Instance.GetScore();
        score.text = $"{currentScore}";

        // ===== 🏆 HIGH SCORE =====
        int best = SaveDataManager.highestScore;
        highestScore.text = $"Highest Score:  {best}";
        UpdateRank(currentScore);
        // ===== 🔥 MAX COMBO =====
        int maxComboValue = GameManager.Instance.GetMaxComboOverall();
        maxCombo.text = $"{maxComboValue}";
        // ===== 🎯 EVALUATE COUNT =====
        perfectText.text = $" {GameManager.Instance.PerfectCount}";
        greatText.text = $" {GameManager.Instance.GreatCount}";
        goodText.text = $" {GameManager.Instance.GoodCount}";
        missText.text = $" {GameManager.Instance.MissCount}";
    }
    void UpdateRank(int currentScore)
    {
        if (ChartManager.Instance == null) return;

        int maxScore = ChartManager.Instance.MaxPerfectScore;

        if (maxScore <= 0) return;

        float percent = (float)currentScore / maxScore * 100f;

        int index = GetRankIndex(percent);

        if (index >= 0 && index < rankSprites.Count)
        {
            rankResultImage.sprite = rankSprites[index];
        }
    }
    int GetRankIndex(float percent)
    {
        for (int i = 0; i < rankThresholds.Count; i++)
        {
            if (percent >= rankThresholds[i])
            {
                return i;
            }
        }

        return rankThresholds.Count - 1;
    }

    void OnReplay()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}