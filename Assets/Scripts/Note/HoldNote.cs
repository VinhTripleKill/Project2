using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public int laneIndex;
    public float startTime;
    public float endTime;
    public float hitLineY;
    private SpriteRenderer fillSR;
    private bool isSuccess = false;
    private float holdStartTime; // thời điểm bắt đầu giữ thật sự
    public Transform bodyBase;
    public Transform bodyFill;
    private float spawnX;
    private float currentFillLength = 0f;
    private bool headJudged = false;
    private bool isHolding = false;
    private bool finished = false;

    public float despawnY = -9.5f;

    public void Initialize(int lane, float start, float end, float hitY)
    {
        laneIndex = lane;
        startTime = start;
        endTime = end;
        hitLineY = hitY;
        isSuccess = false;
        spawnX = transform.position.x;

        headJudged = false;
        isHolding = false;
        finished = false;

        float duration = endTime - startTime;
        float length = GetCurrentLength();

        // base
        bodyBase.localScale = new Vector3(bodyBase.localScale.x, length, 1f);

        // fill = 0
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, 0f, 1f);

        fillSR = bodyFill.GetComponent<SpriteRenderer>();
        SetFillAlpha(1f);

        JudgeManager.Instance.RegisterHold(this);
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsPaused) return; // 🔥 THÊM
        if (GameManager.Instance.IsGameOver) return;
        Move();
        UpdateVisual();
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

     
    }
    void UpdateVisual()
    {
        float length = GetCurrentLength();

        // ===== BASE =====
        bodyBase.localScale = new Vector3(bodyBase.localScale.x, length, 1f);

        // ===== FILL =====

        // ❌ chưa giữ + chưa xong
        if (!isHolding && !finished)
        {
            bodyFill.localScale = new Vector3(bodyFill.localScale.x, 0f, 1f);
            return;
        }

        // ❌ MISS → giữ nguyên fill tại thời điểm fail (KHÔNG reset)
        if (finished && !isSuccess)
        {
            bodyFill.localScale = new Vector3(bodyFill.localScale.x, currentFillLength, 1f);
            return;
        }

        // ✅ CLEAR → full
        if (finished && isSuccess)
        {
            bodyFill.localScale = new Vector3(bodyFill.localScale.x, length, 1f);
            return;
        }

        // ✅ đang giữ → fill dần
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float progress = Mathf.Clamp01((songTime - holdStartTime) / (endTime - startTime));

        float fillLength = length * progress;

        currentFillLength = fillLength; // 🔥 LƯU LẠI

        bodyFill.localScale = new Vector3(bodyFill.localScale.x, fillLength, 1f);
    }
    void LateUpdate()
    {
        if (GameManager.Instance.IsPaused) return; // 🔥 THÊM
        if (GameManager.Instance.IsGameOver) return;
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float tailTime = endTime - songTime;
        float speed = ChartManager.Instance.CurrentScrollSpeed;
        float tailY = hitLineY + tailTime * speed;
        if (tailY < despawnY)
        {
            ObjectPoolingManager.Instance.ReturnHoldNote(gameObject);
        }
    }
    float GetCurrentLength()
    {
        float duration = endTime - startTime;
        float speed = ChartManager.Instance.CurrentScrollSpeed;
        return duration * speed;
    }
    void FillFull()
    {
        float length = GetCurrentLength();
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
    void CheckOverHoldMiss()
    {
        if (finished) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        // 🔥 nếu chưa từng hold → vẫn phải MISS tail
        if (!isHolding && songTime > endTime + JudgeManager.Instance.goodWindow)
        {
            finished = true;
            isSuccess = false;

            GameManager.Instance.ProcessJudgement("MISS");
            JudgeManager.Instance.RemoveHold(laneIndex);
            return;
        }

        // 🔥 đang hold nhưng quá end → MISS
        if (isHolding && songTime > endTime + JudgeManager.Instance.goodWindow)
        {
            finished = true;
            isHolding = false;
            isSuccess = false;

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
        float currentSpeed = ChartManager.Instance.CurrentScrollSpeed;
        float yPosition = hitLineY + timeUntilHit * currentSpeed;
        transform.position = new Vector3(spawnX, yPosition, 0f);
    }

    void CheckAutoMiss()
    {
        if (finished) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (!headJudged && songTime - startTime > 0.3f)
        {
            headJudged = true;
           


            GameManager.Instance.ProcessJudgement("MISS");
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

        float length = GetCurrentLength() * progress;

        currentFillLength = length; // 🔥 LƯU LẠI
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
        currentFillLength = bodyFill.localScale.y;
        if (result != "MISS")
        {
            isSuccess = true;
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