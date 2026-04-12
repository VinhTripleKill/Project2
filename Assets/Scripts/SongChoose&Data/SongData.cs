using UnityEngine;
using UnityEngine.Video;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}


[CreateAssetMenu(fileName = "SongData", menuName = "Music/Song Data")]
public class SongData : ScriptableObject
{

    public string songName;
    public string artistName;
    public Sprite avatar;
    public TextAsset chartData; // hoặc lyrics / beatmap
    public Sprite star1;
    public Sprite star2;
    public Sprite star3;
    [Header("Rank")]
    public Sprite rank;
    public int rankIndex = -1; // 🔥 THÊM (chưa có rank = -1)
    [Header("Star Collected (0-3)")]
    [Range(0, 3)]
    public int starCollected = 0;
    public AudioClip audioClip;
    [Header("Difficulty")]
    public Difficulty difficulty;
    [Header("Video")]
    public VideoClip videoClip; // 🎯 thêm dòng này

    public float duration;
    public int highScore;
}