
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Rhythm/Song")]
public class SongData : ScriptableObject
{
    public string songName;
    public string artist;

    public Sprite cover;

    public AudioClip audio;
    public TextAsset chart;
    public VideoClip video;
}