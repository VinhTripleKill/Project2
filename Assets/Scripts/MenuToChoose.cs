using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuToChoose : MonoBehaviour
{
    public void StartToChooseSong()
    {
        SceneManager.LoadScene("choosesong");
    }

    public void BackToMenu()
    {

        SceneManager.LoadScene("MenuGame");
    }
}