using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SongSelectManager : MonoBehaviour
{
    public SongData[] songs;

    public Transform content;
    public GameObject songItemPrefab;

    public Image cover;
    public Text songName;
    public Text artist;

    public AudioSource previewAudio;

    private SongData selectedSong;

    void Start()
    {
        foreach (var song in songs)
        {
            var obj = Instantiate(songItemPrefab, content);

            var ui = obj.GetComponent<SongItemUI>();
            ui.Setup(song, this);

            obj.GetComponent<Button>().onClick.AddListener(ui.Click);
        }
    }

    public void SelectSong(SongData song)
    {
        selectedSong = song;

        cover.sprite = song.cover;
        songName.text = song.songName;
        artist.text = song.artist;

        previewAudio.Stop();
        previewAudio.clip = song.audio;
        previewAudio.Play();
    }

    public void Play()
    {
        if (selectedSong == null)
        {
            Debug.LogError("No song selected!");
            return;
        }

        
        PlaySceneData.song = selectedSong;

        SceneManager.LoadScene(1);
    }
}