using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public int laneIndex;
    public float startTime;
    public float endTime;

    public float scrollSpeed;
    public float hitLineY;
    private SpriteRenderer fillSR;
    private float totalLength;
    private float holdStartTime; // thời điểm bắt đầu giữ thật sự
    public Transform bodyBase;
    public Transform bodyFill;
    private float spawnX;

    private bool headJudged = false;
    private bool isHolding = false;
    private bool finished = false;

    public float despawnY = -9.5f;

    public void Initialize(int lane, float start, float end, float speed, float hitY)
    {
        laneIndex = lane;
        startTime = start;
        endTime = end;
        scrollSpeed = speed;
        hitLineY = hitY;

        spawnX = transform.position.x;

        headJudged = false;
        isHolding = false;
        finished = false;

        float duration = endTime - startTime;
        totalLength = duration * scrollSpeed;

        // ✅ base full chiều dài
        bodyBase.localScale = new Vector3(bodyBase.localScale.x, totalLength, 1f);

        // ✅ fill bắt đầu = 0
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, 0f, 1f);

        fillSR = bodyFill.GetComponent<SpriteRenderer>();
        SetFillAlpha(1f);

        JudgeManager.Instance.RegisterHold(this);
    }

    void Update()
    {
        Move();

        if (AutoPlayManager.Instance != null &&
            AutoPlayManager.Instance.isAutoPlay)
        {
            AutoPlay();
        }
        else
        {
            CheckAutoMiss();
            CheckOverHoldMiss();
        }
        if (isHolding && !finished)
        {
            UpdateFill();
        }
    }
    void FillFull()
    {
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, totalLength, 1f);
    }
    void UpdateFill()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float progress = Mathf.Clamp01((songTime - holdStartTime) / (endTime - startTime));

        float length = totalLength * progress;

        bodyFill.localScale = new Vector3(bodyFill.localScale.x, length, 1f);
    }
    void AutoPlay()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        // ===== AUTO PRESS =====
        if (!headJudged && songTime >= startTime)
        {
            TryPress(); // sẽ auto PERFECT
            return;
        }

        // ===== AUTO RELEASE =====
        if (isHolding && !finished && songTime >= endTime)
        {
            TryRelease(); // auto PERFECT
        }
    }
    void LateUpdate()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float tailTime = endTime - songTime;
        float tailY = hitLineY + tailTime * scrollSpeed;

        if (tailY < despawnY)
        {
            ObjectPoolingManager.Instance.ReturnHoldNote(gameObject);
        }
    }
    void CheckOverHoldMiss()
    {
        if (!isHolding || finished) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (songTime > endTime + JudgeManager.Instance.goodWindow)
        {
            finished = true;
            isHolding = false;

            GameManager.Instance.ProcessJudgement("MISS");
            JudgeManager.Instance.RemoveHold(laneIndex);
        }
    }
    void SetFillAlpha(float a)
    {
        if (fillSR == null) return;

        Color c = fillSR.color;
        c.a = a;
        fillSR.color = c;
    }
    void Move()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float timeUntilHit = startTime - songTime;
        float yPosition = hitLineY + timeUntilHit * scrollSpeed;

        transform.position = new Vector3(spawnX, yPosition, 0f);
    }

    void CheckAutoMiss()
    {
        if (finished) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (!headJudged && songTime - startTime > 0.3f)
        {
            headJudged = true;
            finished = true;

            GameManager.Instance.ProcessJudgement("MISS");

            // ✅ REMOVE khỏi JudgeManager
            JudgeManager.Instance.RemoveHold(laneIndex);
        }
    }

    public void TryPress()
    {
        if (headJudged) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float diff = Mathf.Abs(songTime - startTime);

        string result = JudgeManager.Instance.GetJudgeResult(diff);

        headJudged = true;

        if (result != "MISS")
        {
            isHolding = true;
            holdStartTime = startTime;
            SnapFillToHitLine();
        }
        else
        {
            finished = true;
        }

        GameManager.Instance.ProcessJudgement(result);
    }
    void SnapFillToHitLine()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float progress = Mathf.Clamp01((songTime - startTime) / (endTime - startTime));

        float length = totalLength * progress;

        bodyFill.localScale = new Vector3(bodyFill.localScale.x, length, 1f);
    }

    public void TryRelease()
    {
        if (!isHolding || finished) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        // ❌ thả trước end
        if (songTime < endTime - JudgeManager.Instance.goodWindow)
        {
            finished = true;
            isHolding = false;

            GameManager.Instance.ProcessJudgement("MISS");
            JudgeManager.Instance.RemoveHold(laneIndex);

            return;
        }

        // ✅ release đúng
        float diff = Mathf.Abs(songTime - endTime);
        string result = JudgeManager.Instance.GetJudgeResult(diff);

        finished = true;
        isHolding = false;

        if (result != "MISS")
        {
            FillFull();
            SetFillAlpha(0.3f); // 🔥 fade
        }

        GameManager.Instance.ProcessJudgement(result);
        JudgeManager.Instance.RemoveHold(laneIndex);
    }
    
    public void ForceMissTail()
    {
        if (!finished)
        {
            finished = true;
            JudgeManager.Instance.RemoveHold(laneIndex);
            ObjectPoolingManager.Instance.ReturnHoldNote(gameObject);
        }
    }

}