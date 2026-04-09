using UnityEngine;
using UnityEngine.Video;

public class PlaySceneManager : MonoBehaviour
{
    private AudioSource audioSource;
    private VideoPlayer videoPlayer;
    private ChartManager chartManager;

    void Awake()
    {
        audioSource = FindAnyObjectByType<AudioSource>();
        videoPlayer = FindAnyObjectByType<VideoPlayer>();
        chartManager = FindAnyObjectByType<ChartManager>();
    }

    void Start()
    {
        var song = PlaySceneData.song;

        if (song == null)
        {
            Debug.LogError("Song NULL");
            return;
        }

        audioSource.clip = song.audio;
        audioSource.Play();

        videoPlayer.clip = song.video;
        videoPlayer.Play();

        chartManager.chartFile = song.chart;
    }
}