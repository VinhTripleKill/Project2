using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    private GameInput input;

    private Action<InputAction.CallbackContext>[] pressActions;
    private Action<InputAction.CallbackContext>[] releaseActions;

    private const int LaneCount = 4;

    void Awake()
    {
        input = new GameInput();

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

        input.Game.Lane0.started += pressActions[0];
        input.Game.Lane0.canceled += releaseActions[0];

        input.Game.Lane1.started += pressActions[1];
        input.Game.Lane1.canceled += releaseActions[1];

        input.Game.Lane2.started += pressActions[2];
        input.Game.Lane2.canceled += releaseActions[2];

        input.Game.Lane3.started += pressActions[3];
        input.Game.Lane3.canceled += releaseActions[3];

        
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

        

        input.Disable();
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
        return AudioManager.Instance != null;
    }
}