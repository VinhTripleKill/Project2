using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance;

    [Header("Buttons")]
    public Button gameplayBtn;
    public Button audioBtn;
    public Button controlBtn;
    public Button graphicsBtn;

    [Header("Panels")]
    public GameObject gameplay_In4;
    public GameObject audio_In4;
    public GameObject control_In4;
    public GameObject graphics_In4;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;

    [Header("Audio Settings")]
    public Toggle musicToggle;
    public Slider musicSlider;

    [Header("Speed Settings")]
    public Button increSpeedButton;
    public Button decreSpeedButton;
    public Scrollbar speedScrollbar;
    public TextMeshProUGUI speed_Text;

    private Button currentButton;

    // Audio
    private float savedVolume = 1f;

    // Speed
    private float speed = 1f;
    private bool isUpdatingScrollbar = false;
    private float[] snapPoints = new float[]
    {
        0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f
    };
    private float snapThreshold = 0.03f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Tab buttons
        gameplayBtn.onClick.AddListener(() => SelectTab(gameplayBtn, gameplay_In4));
        audioBtn.onClick.AddListener(() => SelectTab(audioBtn, audio_In4));
        controlBtn.onClick.AddListener(() => SelectTab(controlBtn, control_In4));
        graphicsBtn.onClick.AddListener(() => SelectTab(graphicsBtn, graphics_In4));

        // Audio listeners
        musicSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicToggle.onValueChanged.AddListener(OnToggleMusic);

        // Speed listeners
        increSpeedButton.onClick.AddListener(IncreaseSpeed);
        decreSpeedButton.onClick.AddListener(DecreaseSpeed);
        speedScrollbar.onValueChanged.AddListener(OnScrollbarChanged);

        // Khởi tạo giá trị ban đầu
        InitAudioUI();
        InitSpeedUI();
    }

    // 🔥 Gọi khi mở Pause Menu
    public void InitDefault()
    {
        SelectTab(gameplayBtn, gameplay_In4);
    }

    private void InitAudioUI()
    {
        musicSlider.value = AudioManager.Instance.audioSource.volume;
        savedVolume = musicSlider.value;
        musicToggle.isOn = savedVolume > 0.01f;
    }

    private void InitSpeedUI()
    {
        speed = GameManager.Instance.SpeedMultiplier; // lấy từ GameManager
        ApplySpeed();
    }

    // ================== AUDIO ==================
    void OnVolumeChanged(float value)
    {
        AudioManager.Instance.audioSource.volume = value;

        if (value > 0.01f)
            savedVolume = value;

        musicToggle.SetIsOnWithoutNotify(value > 0.01f);
    }

    void OnToggleMusic(bool isOn)
    {
        if (isOn)
        {
            float restore = savedVolume > 0 ? savedVolume : 1f;
            AudioManager.Instance.audioSource.volume = restore;
            musicSlider.SetValueWithoutNotify(restore);
        }
        else
        {
            AudioManager.Instance.audioSource.volume = 0f;
            musicSlider.SetValueWithoutNotify(0f);
        }
    }

    // ================== SPEED ==================
    void OnScrollbarChanged(float value)
    {
        if (isUpdatingScrollbar) return;

        float rawSpeed = Mathf.Lerp(0.5f, 2f, value);
        float smoothSpeed = Mathf.Round(rawSpeed * 100f) / 100f;
        float snapped = GetSnappedSpeed(smoothSpeed);

        speed = snapped;
        ApplySpeed();
    }

    float GetSnappedSpeed(float input)
    {
        foreach (float point in snapPoints)
        {
            if (Mathf.Abs(input - point) <= snapThreshold)
                return point;
        }
        return input;
    }

    void IncreaseSpeed()
    {
        speed += 0.25f;
        speed = Mathf.Clamp(speed, 0.5f, 2f);
        ApplySpeed();
    }

    void DecreaseSpeed()
    {
        speed -= 0.25f;
        speed = Mathf.Clamp(speed, 0.5f, 2f);
        ApplySpeed();
    }

    void ApplySpeed()
    {
        GameManager.Instance.SetSpeed(speed); // sẽ tạo method này ở GameManager

        speed_Text.text = "x" + speed.ToString("0.##");

        float normalized = (speed - 0.5f) / (2f - 0.5f);
        isUpdatingScrollbar = true;
        speedScrollbar.SetValueWithoutNotify(normalized);
        isUpdatingScrollbar = false;
    }

    // Các hàm cũ của tab
    void SelectTab(Button btn, GameObject panel)
    {
        ResetButtons();
        HideAllPanels();

        currentButton = btn;
        SetButtonColor(btn, pressedColor);
        panel.SetActive(true);
    }

    void ResetButtons()
    {
        SetButtonColor(gameplayBtn, normalColor);
        SetButtonColor(audioBtn, normalColor);
        SetButtonColor(controlBtn, normalColor);
        SetButtonColor(graphicsBtn, normalColor);
    }

    void HideAllPanels()
    {
        gameplay_In4.SetActive(false);
        audio_In4.SetActive(false);
        control_In4.SetActive(false);
        graphics_In4.SetActive(false);
    }

    void SetButtonColor(Button btn, Color color)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.selectedColor = color;
        cb.pressedColor = color;
        cb.highlightedColor = color;
        btn.colors = cb;
    }
}