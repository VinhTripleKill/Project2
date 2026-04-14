using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class SongItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Root (Image lớn chứa item)")]
    public Image background;

    [Header("UI")]
    public Image avatar;
    public Image star1;
    public Image star2;
    public Image star3;
    public Image rank;
    [SerializeField] private Sprite defaultRankSprite;
    public TMP_Text songNameText;
    public TMP_Text artistNameText;

    private SongData data;
    private Action<SongItemUI, SongData> onClickCallback;

    private Vector3 defaultScale;
    private Color defaultColor;

    [Header("Difficulty UI")]
    public TMP_Text difficultyText;
    public Image difficultyImage;

    [Header("Difficulty Colors")]
    public Color easyColor;
    public Color normalColor;
    public Color hardColor;
    void Awake()
    {
        defaultScale = transform.localScale;
        defaultColor = background.color;
    }

    public void Setup(SongData songData, Action<SongItemUI, SongData> onClick)
    {
        data = songData;
        onClickCallback = onClick;

        avatar.sprite = data.avatar;
        star1.sprite = data.star1;
        star2.sprite = data.star2;
        star3.sprite = data.star3;
        bool hasRank = data.rankIndex != -1;

        rank.gameObject.SetActive(hasRank);

        if (hasRank)
        {
            rank.sprite = data.rank;
        }
        songNameText.text = data.songName;
        artistNameText.text = data.artistName;

        SetupDifficulty();

        // 🎯 thêm dòng này
        SetupStars();
    }
    void SetupStars()
    {
        // mặc định tắt hết (V = 0)
        SetStarV(star1, 0f);
        SetStarV(star2, 0f);
        SetStarV(star3, 0f);

        if (data.starCollected >= 1)
            SetStarV(star1, 1f);

        if (data.starCollected >= 2)
            SetStarV(star2, 1f);

        if (data.starCollected >= 3)
            SetStarV(star3, 1f);
    }
    void SetStarV(Image img, float v)
    {
        Color c = img.color;

        Color.RGBToHSV(c, out float h, out float s, out float oldV);

        c = Color.HSVToRGB(h, s, v);

        // giữ lại alpha cũ
        c.a = img.color.a;

        img.color = c;
    }

    void SetupDifficulty()
    {
        difficultyText.text = data.difficulty.ToString();

        Color color = GetDifficultyColor(data.difficulty);

       
        difficultyImage.color = color;
    }

    Color GetDifficultyColor(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.Easy:
                return easyColor;
            case Difficulty.Normal:
                return normalColor;
            case Difficulty.Hard:
                return hardColor;
        }
        return Color.white;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(this, data);
    }

    // 🎯 SELECT
    public void Select()
    {
        transform.localScale = defaultScale * 1.05f;

        Color c = defaultColor;
        Color.RGBToHSV(c, out float h, out float s, out float v);

        v = 0.7f; // từ 0.5 → 0.7
        background.color = Color.HSVToRGB(h, s, v);
    }

    // 🎯 DESELECT
    public void Deselect()
    {
        transform.localScale = defaultScale;
        background.color = defaultColor;
    }
}