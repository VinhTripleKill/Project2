using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    private float songLength;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            songLength = AudioManager.Instance.audioSource.clip.length;
        }
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return; // 🔥

        if (AudioManager.Instance == null) return;

        double songTime = AudioManager.Instance.SongTimeDSP;

        float progress = (float)(songTime / songLength);
        progressBar.fillAmount = Mathf.Clamp01(progress);
    }
}