using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    [Header("Audio Settings")]
    public Toggle musicToggle;
    public Slider musicSlider;

    [Header("Speed Settings")]
    public Button increSpeedButton;
    public Button decreSpeedButton;
    public Scrollbar speedScrollbar;
    public TextMeshProUGUI speed_Text;

    private Button currentButton;
    [Header("Control Settings")]
    public Button laneInput1;
    public Button laneInput2;
    public Button laneInput3;
    public Button laneInput4;
    private TextMeshProUGUI[] laneTexts;
    [Header("Rebind UI")]
    public GameObject boxInput;
    public TMP_InputField inputField;
    public Button saveButton;
    public Button cancelButton;
    private int currentLaneIndex = -1;
    private string originalKey = "";
    private string currentInput = "";
    private bool hasChanged = false;
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
        int saved = PlayerPrefs.GetInt("Resolution", 2); // default 1920x1080
        resolutionDropdown.value = saved;
        resolutionDropdown.RefreshShownValue();

        SetResolution(saved); // apply luôn
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        laneInput1.onClick.AddListener(() => OnClickLane(0));
        laneInput2.onClick.AddListener(() => OnClickLane(1));
        laneInput3.onClick.AddListener(() => OnClickLane(2));
        laneInput4.onClick.AddListener(() => OnClickLane(3));
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
        InitControlUI();
    }
    void Update()
    {
        if (!boxInput.activeSelf) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverUI())
            {
                if (!hasChanged)
                {
                    ResetBox();
                }
            }
        }
    }
    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 👉 nếu click vào bất kỳ UI nào trong BoxInput
        foreach (var r in results)
        {
            if (r.gameObject.transform.IsChildOf(boxInput.transform))
                return true;
        }

        return false;
    }
    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                break;

            case 1:
                Screen.SetResolution(1600, 900, FullScreenMode.FullScreenWindow);
                break;

            case 2:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;

            case 3:
                Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow);
                break;
        }

        PlayerPrefs.SetInt("Resolution", index);
        PlayerPrefs.Save();
    }
    void OnClickLane(int lane)
    {
        currentLaneIndex = lane;

        originalKey = laneTexts[lane].text;
        currentInput = originalKey;

        inputField.text = originalKey;

        hasChanged = false;

        boxInput.SetActive(true);
        saveButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        inputField.onValueChanged.RemoveAllListeners();
        inputField.onValueChanged.AddListener(OnInputChanged);
    }
    void OnInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            currentInput = "";
            return;
        }

        // 🔥 chỉ lấy 1 ký tự đầu
        value = value.Substring(0, 1).ToUpper();

        inputField.SetTextWithoutNotify(value);
        currentInput = value;

        hasChanged = currentInput != originalKey;

        saveButton.gameObject.SetActive(hasChanged);
        cancelButton.gameObject.SetActive(hasChanged);
    }
    public void OnCancel()
    {
        ResetBox();
    }
    public void OnSave()
    {
        if (!hasChanged) return;

        for (int i = 0; i < laneTexts.Length; i++)
        {
            if (i == currentLaneIndex) continue;

            if (laneTexts[i].text == currentInput)
            {
                Debug.Log("Không thể có 2 input trùng lặp");
                return;
            }
        }

        InputManager.Instance.ApplyRebind(currentLaneIndex, currentInput);

        // 🔥 LƯU
        PlayerPrefs.SetString("LaneKey_" + currentLaneIndex, currentInput);
        PlayerPrefs.Save();

        laneTexts[currentLaneIndex].text = currentInput;

        ResetBox();
    }

    void ResetBox()
    {
        boxInput.SetActive(false);

        currentLaneIndex = -1;
        hasChanged = false;
    }
    void InitControlUI()
    {
        laneTexts = new TextMeshProUGUI[4];

        laneTexts[0] = laneInput1.GetComponentInChildren<TextMeshProUGUI>();
        laneTexts[1] = laneInput2.GetComponentInChildren<TextMeshProUGUI>();
        laneTexts[2] = laneInput3.GetComponentInChildren<TextMeshProUGUI>();
        laneTexts[3] = laneInput4.GetComponentInChildren<TextMeshProUGUI>();

        UpdateLaneKeyTexts();
    }
    void UpdateLaneKeyTexts()
    {
        var input = InputManager.Instance.input;
        var actions = new InputAction[]
        {
        input.Game.Lane0,
        input.Game.Lane1,
        input.Game.Lane2,
        input.Game.Lane3
        };

        for (int i = 0; i < actions.Length; i++)
        {
            string key = actions[i].GetBindingDisplayString();

            // OPTIONAL: format đẹp hơn
            key = key.ToUpper();

            laneTexts[i].text = key;
        }
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