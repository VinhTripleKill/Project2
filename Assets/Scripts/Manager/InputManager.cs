using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public GameInput input { get; private set; }

    private Action<InputAction.CallbackContext>[] pressActions;
    private Action<InputAction.CallbackContext>[] releaseActions;

    private const int LaneCount = 4;

    void Awake()
    {
        input = new GameInput();
        Instance = this;
        LoadBindings();
        pressActions = new Action<InputAction.CallbackContext>[LaneCount];
        releaseActions = new Action<InputAction.CallbackContext>[LaneCount];

        for (int i = 0; i < LaneCount; i++)
        {
            int laneIndex = i;

            pressActions[i] = ctx => OnPress(laneIndex);
            releaseActions[i] = ctx => OnRelease(laneIndex);
        }
    }

    void OnEnable()
    {
        input.Enable();

        // Lane (giữ nguyên)
        input.Game.Lane0.started += pressActions[0];
        input.Game.Lane0.canceled += releaseActions[0];

        input.Game.Lane1.started += pressActions[1];
        input.Game.Lane1.canceled += releaseActions[1];

        input.Game.Lane2.started += pressActions[2];
        input.Game.Lane2.canceled += releaseActions[2];

        input.Game.Lane3.started += pressActions[3];
        input.Game.Lane3.canceled += releaseActions[3];

        // 🔥 PAUSE
        input.UI.PauseGame.performed += OnPausePressed;
    }
    void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.IsResultShowing) return;
        GameManager.Instance.ToggleStatus();
    }

    void OnDisable()
    {
        input.Game.Lane0.started -= pressActions[0];
        input.Game.Lane0.canceled -= releaseActions[0];

        input.Game.Lane1.started -= pressActions[1];
        input.Game.Lane1.canceled -= releaseActions[1];

        input.Game.Lane2.started -= pressActions[2];
        input.Game.Lane2.canceled -= releaseActions[2];

        input.Game.Lane3.started -= pressActions[3];
        input.Game.Lane3.canceled -= releaseActions[3];

        input.UI.PauseGame.performed -= OnPausePressed;

        input.Disable();
    }
    void LoadBindings()
    {
        for (int i = 0; i < 4; i++)
        {
            string key = PlayerPrefs.GetString("LaneKey_" + i, "");

            if (string.IsNullOrEmpty(key)) continue;

            ApplyRebind(i, key);
        }
    }
    public void ApplyRebind(int lane, string key)
    {
        InputAction action = null;

        switch (lane)
        {
            case 0: action = input.Game.Lane0; break;
            case 1: action = input.Game.Lane1; break;
            case 2: action = input.Game.Lane2; break;
            case 3: action = input.Game.Lane3; break;
        }

        string path = $"<Keyboard>/{key.ToLower()}";
        action.ApplyBindingOverride(0, path);
    }
    void OnPress(int lane)
    {
        if (!IsGameplayActive()) return;
        LaneManager.Instance.PressLane(lane);
        if (AutoPlayManager.Instance != null &&
            AutoPlayManager.Instance.isAutoPlay)
            return;

        JudgeManager.Instance.OnPress(lane);
    }

    void OnRelease(int lane)
    {
        if (!IsGameplayActive()) return;
        LaneManager.Instance.ReleaseLane(lane);
        if (AutoPlayManager.Instance != null &&
            AutoPlayManager.Instance.isAutoPlay)
            return;

        JudgeManager.Instance.OnRelease(lane);
    }

    bool IsGameplayActive()
    {
        return AudioManager.Instance != null
            && GameManager.Instance != null
            //&& GameManager.Instance.IsGameStarted
            && !GameManager.Instance.IsPaused
            && !GameManager.Instance.IsGameOver
            && !GameManager.Instance.IsResultShowing
            && !GameManager.Instance.IsResumeCoolingDown();


    }
}