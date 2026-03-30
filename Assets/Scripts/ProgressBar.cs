using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressBar;

    [Header("Star Points")]
    [SerializeField] private Image starPointPrefab;
    [SerializeField] private RectTransform starParent;
    private bool[] starCollected;
    private float songLength;

    private List<Image> starPoints = new List<Image>();
    private float[] starPercents = new float[] { 0.3334f, 0.6667f, 1f };

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            songLength = AudioManager.Instance.audioSource.clip.length;
        }
        starCollected = new bool[starPercents.Length];
        SpawnStarPoints();
    }

    void SpawnStarPoints()
    {
        RectTransform barRect = progressBar.GetComponent<RectTransform>();

        foreach (float percent in starPercents)
        {
            Image star = Instantiate(starPointPrefab, starParent);

            RectTransform rt = star.GetComponent<RectTransform>();

            // anchor theo thanh progress
            rt.anchorMin = new Vector2(percent, 0.5f);
            rt.anchorMax = new Vector2(percent, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            SetStarValue(star, 0f); // ban đầu tối

            starPoints.Add(star);
        }
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

    void UpdateStars(float progress)
    {
        for (int i = 0; i < starPoints.Count; i++)
        {
            // ⭐ ĐÃ CHẠM MỐC
            if (progress >= starPercents[i])
            {
                SetStarValue(starPoints[i], 1f);

                // 🔥 trigger 1 lần duy nhất
                if (!starCollected[i])
                {
                    starCollected[i] = true;
                    Debug.Log($"StarPoint {i + 1} have collected");
                }
            }
            else
            {
                SetStarValue(starPoints[i], 0f);
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