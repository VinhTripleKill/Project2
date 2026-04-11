using UnityEngine;
using UnityEngine.EventSystems;

public class SongListManager : MonoBehaviour, IPointerClickHandler
{
    public SongData[] songs;
    public SongItemUI itemPrefab;
    public Transform content;

    public SongDetailUI detailUI;

    private SongItemUI currentSelected;

    void Start()
    {
        foreach (var song in songs)
        {
            var item = Instantiate(itemPrefab, content);
            item.Setup(song, OnItemClicked);
        }
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