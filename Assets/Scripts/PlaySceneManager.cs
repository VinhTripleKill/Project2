using UnityEngine;
using UnityEngine.Video;

public class PlaySceneManager : MonoBehaviour
{
    void Start()
    {
        var song = PlaySceneData.song;

        if (song == null)
        {
            Debug.LogError("Song NULL");
            return;
        }

        // AUDIO
        var audio = FindAnyObjectByType<AudioSource>();
        audio.clip = song.audio;
        audio.Play();

        // CHART
        var chart = FindAnyObjectByType<ChartManager>();
        chart.LoadChartFromSong(song.chart);

        // VIDEO (tu BG)
        var video = GameObject
            .Find("backGroundManager/BG")
            .GetComponent<VideoPlayer>();

        video.clip = song.video;
        video.Play();
    }
}