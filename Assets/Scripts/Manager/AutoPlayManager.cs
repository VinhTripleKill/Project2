using UnityEngine;
using UnityEngine.UI;

public class AutoPlayManager : MonoBehaviour
{
    public static AutoPlayManager Instance;

    public bool isAutoPlay = false;

    public Button autoButton;
    private Image buttonImage;

    void Awake()
    {
        Instance = this;
        buttonImage = autoButton.GetComponent<Image>();
        
    }
    void Start()
    {
        if (GameManager.Instance != null)
        {
            isAutoPlay = GameManager.Instance.playMode == ModePlay.Auto;
        }

        UpdateButtonColor();
    }
    public void ToggleAutoPlay()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonColor();
    }

    void UpdateButtonColor()
    {
        if (isAutoPlay)
        {
            buttonImage.color = new Color(1f, 0f, 0f, 0.7f); // đỏ trong suốt
        }
        else
        {
            buttonImage.color = Color.white;
        }
    }
}