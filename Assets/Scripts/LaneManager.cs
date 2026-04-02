using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance;

    [System.Serializable]
    public class Lane
    {
        public SpriteRenderer visualLane;
    }

    public Lane[] lanes;

    [Header("Color Settings")]
    public Color idleColor = Color.white;        // màu bình thường
    public Color interactive;                    // màu khi nhấn

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetLanes();
    }

    void ResetLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            SetColor(lanes[i].visualLane, idleColor);
        }
    }

    public void PressLane(int lane)
    {
        if (lane < 0 || lane >= lanes.Length) return;

        SetColor(lanes[lane].visualLane, interactive);
    }

    public void ReleaseLane(int lane)
    {
        if (lane < 0 || lane >= lanes.Length) return;

        SetColor(lanes[lane].visualLane, idleColor);
    }

    void SetColor(SpriteRenderer sr, Color color)
    {
        sr.color = color;
    }
}