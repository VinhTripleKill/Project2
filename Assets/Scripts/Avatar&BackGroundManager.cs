using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class AvatarBackgroundManager : MonoBehaviour
{
    public Image avatarImage;

    [Header("BG")]
    public Image backgroundImage;   // ảnh avatar
    public RawImage videoRawImage;  // video
    public VideoPlayer videoPlayer;

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    private bool startedVideo = false;
    private bool endedVideo = false;

    private Coroutine fadeRoutine;

    void Start()
    {
        // sync avatar → BG
        if (avatarImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = avatarImage.sprite;
        }

        SetAlpha(backgroundImage, 1f);
        SetAlpha(videoRawImage, 0f);

        videoPlayer.Stop();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // ===== CHƯA START =====
        if (!GameManager.Instance.IsGameStarted)
        {
            return;
        }

        // ===== START VIDEO (fade vào) =====
        if (!startedVideo)
        {
            StartVideoWithFade();
        }

        // ===== PAUSE =====
        if (GameManager.Instance.IsPaused||GameManager.Instance.IsGameOver)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
        }
        else
        {
            if (!videoPlayer.isPlaying && startedVideo && !endedVideo)
                videoPlayer.Play();
        }

        // ===== KẾT THÚC NHẠC → FADE NGƯỢC =====
        if (!endedVideo && IsSongFinished())
        {
            EndVideoWithFade();
        }
    }

    // =========================
    // 🎬 START VIDEO (fade in)
    // =========================
    void StartVideoWithFade()
    {
        startedVideo = true;

        videoRawImage.enabled = true;
        videoPlayer.Play();

        StartFade(backgroundImage, videoRawImage);
    }

    // =========================
    // 🎬 END VIDEO (fade out)
    // =========================
    void EndVideoWithFade()
    {
        endedVideo = true;

        StartFade(videoRawImage, backgroundImage);
    }

    // =========================
    // 🎨 FADE CORE
    // =========================
    void StartFade(Graphic from, Graphic to)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(from, to));
    }

    IEnumerator FadeRoutine(Graphic from, Graphic to)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            SetAlpha(from, 1f - t);
            SetAlpha(to, t);

            yield return null;
        }

        SetAlpha(from, 0f);
        SetAlpha(to, 1f);

        // tắt hẳn cái bị fade out
        from.enabled = false;
    }

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;

        Color c = g.color;
        c.a = a;
        g.color = c;

        if (!g.enabled) g.enabled = true;
    }

    // =========================
    // 🎵 CHECK END NHẠC
    // =========================
    bool IsSongFinished()
    {
        if (AudioManager.Instance == null) return false;

        float length = AudioManager.Instance.audioSource.clip.length;
        double time = AudioManager.Instance.SongTimeDSP;

        return time >= length;
    }
}