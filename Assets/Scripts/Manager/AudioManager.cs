using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;
    public float globalOffset = 0f;
    private double pauseDSPTime;
    private double songStartDSPTime;
    private bool isPlaying = false;
    private double pausedSongTime; // 🔥 THÊM


    void Awake()
    {
        Instance = this;

        audioSource.Stop(); // 🔥 đảm bảo không auto play
    }
    public double SongTimeDSP
    {
        get
        {
            if (!isPlaying)
                return pausedSongTime;

            return (AudioSettings.dspTime - songStartDSPTime) + globalOffset;
        }
    }

    public void PlaySong()
    {
        songStartDSPTime = AudioSettings.dspTime; // 🔥 KHÔNG delay nữa
        audioSource.Play(); // play ngay lập tức
        isPlaying = true;
    }
    public void PauseSong()
    {
        if (!isPlaying) return;

        pauseDSPTime = AudioSettings.dspTime;

        // 🔥 LƯU LẠI THỜI GIAN HIỆN TẠI
        pausedSongTime = SongTimeDSP;

        audioSource.Pause();
        isPlaying = false;
    }
    public void ResumeSong()
    {
        if (isPlaying) return;

        double pausedDuration = AudioSettings.dspTime - pauseDSPTime;

        // 🔥 bù lại thời gian đã pause
        songStartDSPTime += pausedDuration;

        audioSource.UnPause();
        isPlaying = true;
    }
    public IEnumerator FadeOutPitchThenStop(float duration)
    {
        float startPitch = audioSource.pitch;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            audioSource.pitch = Mathf.Lerp(startPitch, 0f, t);
            yield return null;
        }

        audioSource.pitch = 0f;

        // ⛔ stop sau khi fade xong
        audioSource.Stop();

        // 🔥 lúc này mới freeze game hoàn toàn
        Time.timeScale = 0f;
    }

}