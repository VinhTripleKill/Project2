using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // nhớ import

public class SongListManager : MonoBehaviour, IPointerClickHandler
{
    public SongData[] songs;
    public SongItemUI itemPrefab;
    public Transform content;

    public Button resetButton;
    public SongDetailUI detailUI;

    private SongItemUI currentSelected;
    [SerializeField] private List<Sprite> rankSprites;

    Sprite GetRankSprite(int index)
    {
        if (index >= 0 && index < rankSprites.Count)
            return rankSprites[index];

        return null;
    }
    void Start()
    {
        SaveDataManager.Load();

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetData);

        foreach (var song in songs)
        {
            // 🔥 RESET TRƯỚC (fix bug ScriptableObject giữ state)
            song.rank = null;
            song.rankIndex = -1;

            song.highScore = SaveDataManager.GetHighScore(song.songName);
            song.starCollected = SaveDataManager.GetStars(song.songName);

            int rankIndex = SaveDataManager.GetRank(song.songName);

            if (rankIndex != -1)
            {
                song.rankIndex = rankIndex;
                song.rank = GetRankSprite(rankIndex);
            }

            var item = Instantiate(itemPrefab, content);
            item.Setup(song, OnItemClicked);
        }
    }
    void OnResetData()
    {
        Debug.Log("🔥 RESET DATA CLICKED");

        SaveDataManager.DeleteAllData();

        // 🔥 reload scene cho sạch UI
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void OnItemClicked(SongItemUI item, SongData song)
    {
        if (currentSelected != null)
            currentSelected.Deselect();

        currentSelected = item;
        currentSelected.Select();

        // 🎯 truyền cả item
        detailUI.Show(song, item);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // nếu click trúng item thì bỏ qua
        if (eventData.pointerPress != null &&
            eventData.pointerPress.GetComponentInParent<SongItemUI>() != null)
            return;

        if (currentSelected != null)
        {
            currentSelected.Deselect();
            currentSelected = null;
            detailUI.Hide();
        }
    }
}