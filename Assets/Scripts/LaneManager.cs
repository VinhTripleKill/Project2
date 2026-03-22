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

    [Range(0f, 1f)]
    public float idleAlpha = 0.2f;

    [Range(0f, 1f)]
    public float pressedAlpha = 1f;

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
            SetAlpha(lanes[i].visualLane, idleAlpha);
        }
    }

    public void PressLane(int lane)
    {
        if (lane < 0 || lane >= lanes.Length) return;

        SetAlpha(lanes[lane].visualLane, pressedAlpha);
    }

    public void ReleaseLane(int lane)
    {
        if (lane < 0 || lane >= lanes.Length) return;

        SetAlpha(lanes[lane].visualLane, idleAlpha);
    }

    void SetAlpha(SpriteRenderer sr, float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}