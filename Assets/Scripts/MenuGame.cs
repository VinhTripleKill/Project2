using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuGameUI : MonoBehaviour
{
    public Button startGameButton;
    public Button continueButton;
    public Button quitButton;

    void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);
    }

    // 🎮 START GAME (xóa dữ liệu)
    void OnStartGame()
    {
        Debug.Log("🔥 START GAME (RESET DATA)");

        SaveDataManager.DeleteAllData(); // xóa save

        SceneManager.LoadScene("ChooseSong");
    }

    // ▶️ CONTINUE (giữ dữ liệu)
    void OnContinue()
    {
        Debug.Log("▶️ CONTINUE GAME");

        SceneManager.LoadScene("ChooseSong");
    }

    // ❌ QUIT GAME
    void OnQuitGame()
    {
        Debug.Log("❌ QUIT GAME");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}