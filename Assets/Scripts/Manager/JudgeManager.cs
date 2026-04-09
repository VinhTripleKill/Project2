using UnityEngine;
using System.Collections.Generic;

public class JudgeManager : MonoBehaviour
{
    public static JudgeManager Instance;

    public float perfectWindow = 0.05f;
    public float greatWindow = 0.1f;
    public float goodWindow = 0.2f;

    private Dictionary<int, Queue<TapNote>> tapNotes = new();
    private Dictionary<int, Queue<HoldNote>> holdNotes = new();
    private Dictionary<int, Queue<SlidePoint>> slideNodes = new();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < 4; i++)
        {
            tapNotes[i] = new Queue<TapNote>();
            holdNotes[i] = new Queue<HoldNote>();
            slideNodes[i] = new Queue<SlidePoint>();
        }
    }

    // ================= REGISTER =================

    public void RegisterSlideNode(SlidePoint node)
    {
        slideNodes[node.laneIndex].Enqueue(node);
    }

    public void RemoveSlideNode(int lane)
    {
        if (slideNodes[lane].Count > 0)
            slideNodes[lane].Dequeue();
    }

    public void RegisterNote(TapNote note)
    {
        tapNotes[note.laneIndex].Enqueue(note);
    }

    public void RegisterHold(HoldNote hold)
    {
        holdNotes[hold.laneIndex].Enqueue(hold);
    }

    public void OnPress(int lane)
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        SlidePoint slideTarget = null;
        HoldNote holdTarget = null;
        TapNote tapTarget = null;

        float slideDiff = float.MaxValue;
        float holdDiff = float.MaxValue;
        float tapDiff = float.MaxValue;

        // ===== SLIDE =====
        if (slideNodes[lane].Count > 0)
        {
            foreach (var node in slideNodes[lane])
            {
                if (node.IsLocked())
                    continue;

                float diff = Mathf.Abs(songTime - node.hitTime);

                if (diff < slideDiff)
                {
                    slideDiff = diff;
                    slideTarget = node;
                }
            }
        }

        // ===== HOLD =====
        if (holdNotes[lane].Count > 0)
        {
            HoldNote hold = holdNotes[lane].Peek();
            holdDiff = Mathf.Abs(songTime - hold.startTime);
            holdTarget = hold;
        }

        // ===== TAP =====
        if (tapNotes[lane].Count > 0)
        {
            TapNote tap = tapNotes[lane].Peek();
            tapDiff = Mathf.Abs(songTime - tap.hitTime);
            tapTarget = tap;
        }

        // ===== tìm note gần nhất =====
        float best = Mathf.Min(slideDiff, holdDiff, tapDiff);

        if (best > goodWindow)
        {
            GameManager.Instance.ProcessJudgement("MISS");
            return;
        }

        // ===== xử lý =====

        if (best == slideDiff && slideTarget != null)
        {
            slideTarget.JudgeHit(GetJudgeResult(slideDiff));
            return;
        }

        if (best == holdDiff && holdTarget != null)
        {
            holdTarget.TryPress();
            return;
        }

        if (best == tapDiff && tapTarget != null)
        {
            tapTarget.JudgeHit(GetJudgeResult(tapDiff));
            return;
        }
        TryJudgeTap(lane);
    }


    // ================= RELEASE =================

    public void OnRelease(int lane)
    {
        if (holdNotes[lane].Count > 0)
        {
            HoldNote hold = holdNotes[lane].Peek();
            hold.TryRelease();
        }
    }

    // ================= TAP =================

    void TryJudgeTap(int lane)
    {
        if (tapNotes[lane].Count == 0)
        {
            GameManager.Instance.ProcessJudgement("MISS");
            return;
        }

        TapNote note = tapNotes[lane].Peek();
        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float diff = Mathf.Abs(songTime - note.hitTime);

        string result = GetJudgeResult(diff);

        if (result != "MISS")
        {
            note.JudgeHit(result);
        }
        else
        {
            GameManager.Instance.ProcessJudgement("MISS");
        }
    }

    // ================= REMOVE =================

    public void RemoveTap(int lane)
    {
        if (tapNotes[lane].Count > 0)
            tapNotes[lane].Dequeue();
    }

    public void RemoveHold(int lane)
    {
        if (holdNotes[lane].Count > 0)
            holdNotes[lane].Dequeue();
    }

    // ================= JUDGE =================

    public string GetJudgeResult(float diff)
    {
        if (diff <= perfectWindow) return "PERFECT";
        if (diff <= greatWindow) return "GREAT";
        if (diff <= goodWindow) return "GOOD";
        return "MISS";
    }
}