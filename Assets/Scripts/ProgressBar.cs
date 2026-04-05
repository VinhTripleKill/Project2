using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    private bool[] previousCollected; // 🔥 star từ lần trước
    [Header("Star Points")]
    [SerializeField] private Image starPointPrefab;
    [SerializeField] private RectTransform starParent;
    private bool[] starCollected;
    private float songLength;
    private int currentRunStarCount = 0;
    private List<Image> starPoints = new List<Image>();
    private float[] starPercents = new float[] { 0.3334f, 0.6667f, 1f };

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            songLength = AudioManager.Instance.audioSource.clip.length;
        }

        starCollected = new bool[starPercents.Length];

        // 🔥 LOAD DATA CŨ
        if (StarSaveData.hasData)
        {
            previousCollected = (bool[])StarSaveData.collected.Clone();
        }
        else
        {
            previousCollected = new bool[starPercents.Length];
        }

        SpawnStarPoints();
    }
    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver) return;
        if (AudioManager.Instance == null) return;

        double songTime = AudioManager.Instance.SongTimeDSP;

        float progress = (float)(songTime / songLength);
        progress = Mathf.Clamp01(progress);

        progressBar.fillAmount = progress;

        UpdateStars(progress);
    }

    void SpawnStarPoints()
    {
        foreach (float percent in starPercents)
        {
            Image star = Instantiate(starPointPrefab, starParent);

            RectTransform rt = star.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(percent, 0.5f);
            rt.anchorMax = new Vector2(percent, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            starPoints.Add(star);
        }

        // 🔥 APPLY trạng thái ban đầu
        ApplyInitialStarState();
    }
    void ApplyInitialStarState()
    {
        for (int i = 0; i < starPoints.Count; i++)
        {
            if (previousCollected[i])
            {
                // ⭐ đã từng đạt → 50%
                SetStarValue(starPoints[i], 0.5f);
            }
            else
            {
                SetStarValue(starPoints[i], 0f);
            }
        }
    }

    public bool[] GetCurrentStars()
    {
        return (bool[])starCollected.Clone();
    }
    void UpdateStars(float progress)
    {
        for (int i = 0; i < starPoints.Count; i++)
        {
            if (progress >= starPercents[i])
            {
                // 🔥 nếu đã trigger rồi thì bỏ qua
                if (starCollected[i]) continue;

                starCollected[i] = true;

                // ===== CASE 1: đã thu thập từ trước =====
                if (previousCollected[i])
                {
                    SetStarValue(starPoints[i], 0.5f);
                    Debug.Log("⭐ Star này bạn đã thu thập rồi");
                }
                else
                {
                    // ===== CASE 2: star mới =====
                    currentRunStarCount++;

                    SetStarValue(starPoints[i], 1f);
                    Debug.Log($"✨ Bạn đã thu thập {currentRunStarCount} star");
                }
            }
        }
    }

    void SetStarValue(Image star, float value)
    {
        Color c = star.color;

        Color.RGBToHSV(c, out float h, out float s, out float v);

        v = value; // 🔥 chỉnh Value

        star.color = Color.HSVToRGB(h, s, v);
    }
}