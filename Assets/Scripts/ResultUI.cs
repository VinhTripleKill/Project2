using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    public Button replayButton;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI total_stars_collected;
    [SerializeField] private TextMeshProUGUI durationTimePlay;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI highestScore;

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
    }

    void OnReplay()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}