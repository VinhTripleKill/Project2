using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class AvatarBackgroundManager : MonoBehaviour
{
    public enum BG_Mode
    {
        Image,
        Video
    }

    [Header("Mode")]
    public BG_Mode mode = BG_Mode.Video;

    public Image avatarImage;

    [Header("BG")]
    public Image backgroundImage;
    public RawImage videoRawImage;
    public VideoPlayer videoPlayer;

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    private bool startedVideo = false;
    private bool endedVideo = false;
    private bool videoReady = false;

    private Coroutine fadeRoutine;

    void Start()
    {
        if (PlaySessionManager.currentSong != null)
        {
            avatarImage.sprite = PlaySessionManager.currentSong.avatar;
            videoPlayer.clip = PlaySessionManager.currentSong.videoClip;
        }
        // sync avatar → BG
        if (avatarImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = avatarImage.sprite;
        }

        // ===== MODE IMAGE =====
        if (mode == BG_Mode.Image)
        {
            backgroundImage.enabled = true;
            videoRawImage.enabled = false;
            videoPlayer.gameObject.SetActive(false);
            return;
        }

        // ===== MODE VIDEO =====
        SetAlpha(backgroundImage, 1f);
        SetAlpha(videoRawImage, 0f);

        videoRawImage.enabled = false;

        // 🔥 QUAN TRỌNG: prepare trước video
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        videoReady = true;

        // gán texture ngay để tránh frame xanh
        videoRawImage.texture = vp.texture;
    }

    void Update()
    {
        if (mode == BG_Mode.Image) return;

        if (GameManager.Instance == null) return;

        if (!GameManager.Instance.IsGameStarted)
            return;

        // ===== START VIDEO =====
        if (!startedVideo && videoReady)
        {
            StartVideoWithFade();
        }

        // ===== PAUSE =====
        if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
        }
        else
        {
            if (!videoPlayer.isPlaying && startedVideo && !endedVideo)
                videoPlayer.Play();
        }

        // ===== END =====
        if (!endedVideo && IsSongFinished())
        {
            EndVideoWithFade();
        }
    }

    // =========================
    void StartVideoWithFade()
    {
        startedVideo = true;

        videoRawImage.enabled = true;

        // 🔥 đảm bảo đã có frame trước khi play
        videoPlayer.Play();

        StartFade(backgroundImage, videoRawImage);
    }

    void EndVideoWithFade()
    {
        endedVideo = true;

        StartFade(videoRawImage, backgroundImage);
    }

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

        // 🔥 đảm bảo object đang bật
        from.enabled = true;
        to.enabled = true;

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

        from.enabled = false;
    }

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;

        Color c = g.color;
        c.a = a;
        g.color = c;
    }

    bool IsSongFinished()
    {
        if (AudioManager.Instance == null) return false;

        float length = AudioManager.Instance.audioSource.clip.length;
        double time = AudioManager.Instance.SongTimeDSP;

        return time >= length;
    }
}