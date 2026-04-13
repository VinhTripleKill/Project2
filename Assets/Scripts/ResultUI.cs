using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button nextButton;
    [Header("Rank System")]
    [SerializeField] private List<float> rankThresholds; // % (0 → 100)
    [SerializeField] private List<Sprite> rankSprites;
    [SerializeField] private Image avatarResult;
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
    [SerializeField] private TextMeshProUGUI nameSong;     // 🔥 THÊM
    [SerializeField] private TextMeshProUGUI nameArtist;   // 🔥 THÊM
    [SerializeField] private TextMeshProUGUI maxCombo;
    void Start()
    {
        replayButton.onClick.AddListener(OnReplay);
        nextButton.onClick.AddListener(OnNext); // 🔥 THÊM
        UpdateResultInfo();
    }

    void UpdateResultInfo()
    {
        if (PlaySessionManager.currentSong == null) return;

        var song = PlaySessionManager.currentSong;

        // ===== 🎵 SONG INFO =====
        nameSong.text = song.songName;
        nameArtist.text = song.artistName;
        avatarResult.sprite = song.avatar;

        // ===== 🎯 SCORE =====
        int currentScore = GameManager.Instance.GetScore();
        score.text = $"{currentScore}";

        // ===== 🏆 HIGH SCORE (THEO SONG) =====
        highestScore.text = $"Highest Score: {song.highScore}";

        int newRankIndex = UpdateRank(currentScore);

        // 🔥 UPDATE RANK CHO SONG
        UpdateSongRank(newRankIndex);
        // ===== ⭐ STAR =====
        int stars = GameManager.Instance.GetCurrentRunStars();
        total_stars_collected.text = $"Stars: {stars}";

        // ===== ⏱ TIME =====
        float time = GameManager.Instance.FinalPlayTime;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        durationTimePlay.text = $"Time: {minutes:00}:{seconds:00}";

        // ===== 🔥 COMBO =====
        maxCombo.text = $"{GameManager.Instance.GetMaxComboOverall()}";

        // ===== 🎯 EVALUATE =====
        perfectText.text = $" {GameManager.Instance.PerfectCount}";
        greatText.text = $" {GameManager.Instance.GreatCount}";
        goodText.text = $" {GameManager.Instance.GoodCount}";
        missText.text = $" {GameManager.Instance.MissCount}";
    }
    void UpdateSongRank(int newRankIndex)
    {
        if (PlaySessionManager.currentSong == null) return;
        if (newRankIndex < 0) return;

        var song = PlaySessionManager.currentSong;

        // 🔥 FIX LOGIC
        if (song.rankIndex == -1 || newRankIndex < song.rankIndex)
        {
            song.rankIndex = newRankIndex;
            song.rank = rankSprites[newRankIndex];
            SaveDataManager.SaveRank(song.songName, newRankIndex);
            Debug.Log($"🏆 New Best Rank: {newRankIndex}");
        }
    }
    int UpdateRank(int currentScore)
    {
        if (ChartManager.Instance == null) return -1;

        int maxScore = ChartManager.Instance.MaxPerfectScore;
        if (maxScore <= 0) return -1;

        float percent = (float)currentScore / maxScore * 100f;

        int index = GetRankIndex(percent);

        if (index >= 0 && index < rankSprites.Count)
        {
            rankResultImage.sprite = rankSprites[index];
        }

        return index; // 🔥 TRẢ VỀ
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
    void OnNext()
    {
        Time.timeScale = 1f;
        PlaySessionManager.Clear(); // optional (reset session)

        SceneManager.LoadScene("ChooseSong");
    }
}