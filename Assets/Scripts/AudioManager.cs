using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;
    public float globalOffset = 0f;

    private double songStartDSPTime;
    private bool isPlaying = false;

    public double SongTimeDSP
    {
        get
        {
            if (!isPlaying) return 0; // 🔥 đổi từ 0 → -1
            return (AudioSettings.dspTime - songStartDSPTime) + globalOffset;
        }
    }

    void Awake()
    {
        Instance = this;

        audioSource.Stop(); // 🔥 đảm bảo không auto play
    }

    public void PlaySong()
    {
        songStartDSPTime = AudioSettings.dspTime; // 🔥 KHÔNG delay nữa
        audioSource.Play(); // play ngay lập tức
        isPlaying = true;
    }
}