using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    public Button replayButton;

    void Start()
    {
        replayButton.onClick.AddListener(OnReplay);
    }

    void OnReplay()
    {
        // 🔥 reset TimeScale (quan trọng)
        Time.timeScale = 1f;

        // 🔥 load lại scene hiện tại
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}