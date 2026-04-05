using UnityEngine;
using UnityEngine.UI;

public class SongItemUI : MonoBehaviour
{
    public Image cover;

    private SongData data;
    private SongSelectManager manager;

    public void Setup(SongData song, SongSelectManager m)
    {
        data = song;
        manager = m;
        cover.sprite = song.cover;
    }

    public void Click()
    {
        manager.SelectSong(data);
    }
}