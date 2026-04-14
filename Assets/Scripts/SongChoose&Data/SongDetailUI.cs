using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongDetailUI : MonoBehaviour
{
    [Header("UI")]
    public Image avatar;
    public Image star1;
    public Image star2;
    public Image star3;
    public Image rank;
    public TMP_Text difficultyText;
    public Image difficultyImage;
    public TMP_Text songNameText;
    public TMP_Text artistNameText;
    public TMP_Text durationText;
    public TMP_Text highScoreText;

    public AudioSource audioSource;
    private SongData currentData;
    [Header("Rotate Settings")]
    public float rotateSpeed = 50f; // tốc độ xoay

    private bool isPlaying = false;

    public void Show(SongData data, SongItemUI itemUI)
    {
        gameObject.SetActive(true);
        currentData = data;
        avatar.sprite = data.avatar;

        star1.sprite = data.star1;
        star2.sprite = data.star2;
        star3.sprite = data.star3;
        bool hasRank = data.rankIndex != -1;

        rank.gameObject.SetActive(hasRank);

        if (hasRank && data.rank != null)
        {
            rank.sprite = data.rank;
        }
        // copy alpha star
        star1.color = itemUI.star1.color;
        star2.color = itemUI.star2.color;
        star3.color = itemUI.star3.color;

        // 🎯 copy difficulty TEXT + COLOR từ item
        difficultyText.text = itemUI.difficultyText.text;
        difficultyText.color = itemUI.difficultyText.color;

        difficultyImage.color = itemUI.difficultyImage.color;

        songNameText.text = data.songName;
        artistNameText.text = data.artistName;

        durationText.text = FormatTime(data.audioClip.length);
        highScoreText.text = data.highScore.ToString();

        audioSource.clip = data.audioClip;
        audioSource.Play();

        avatar.transform.rotation = Quaternion.identity;

        isPlaying = true;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        audioSource.Stop();
        isPlaying = false;
    }

    void Update()
    {
        if (isPlaying && avatar != null)
        {
            avatar.transform.eulerAngles += new Vector3(0, 0, -rotateSpeed * Time.deltaTime);
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void OnClickPlayGame()
    {
        if (currentData == null) return;

        PlaySessionManager.SetSong(currentData); // 🔥 GỌN 1 DÒNG

        SceneManager.LoadScene("PlayGame");
    }
}