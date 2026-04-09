using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public enum ModePlay
    {
        NotVisualByJudge,
        VisualByJudge
    }
    public enum HoldState
    {
        Waiting,        // Chưa đến thời gian head
        HeadPending,    // Đang ở vùng head (có thể press)
        Holding,        // Đang giữ thành công
        Finished,       // Đã xong (success hoặc miss)
        Missed          // Miss hoàn toàn (không thể cứu)
    }
    public int laneIndex;
    public float startTime;
    public float endTime;
    public float hitLineY;
    [Header("Mode")]
    public ModePlay modePlay = ModePlay.NotVisualByJudge;
    [Header("Visual")]
    public Transform bodyBase;
    public Transform bodyFill;
    private SpriteRenderer fillSR;

    private float spawnX;
    private float currentFillLength = 0f;
    public float despawnY = -9.5f;

    // ==================== STATE ====================
    private HoldState currentState = HoldState.Waiting;
    private bool isSuccess = false;

    private float holdStartTime; // chỉ dùng để tính visual nếu cần

    private void Awake()
    {
        fillSR = bodyFill.GetComponent<SpriteRenderer>();
    }

    public void Initialize(int lane, float start, float end, float hitY)
    {
        laneIndex = lane;
        startTime = start;
        endTime = end;
        hitLineY = hitY;
        spawnX = transform.position.x;

        currentState = HoldState.Waiting;
        isSuccess = false;
        currentFillLength = 0f;
        holdStartTime = 0f;
        // Thiết lập scale ban đầu dựa trên mode
        float visualDuration;

        if (modePlay == ModePlay.VisualByJudge)
        {
            visualDuration = (endTime - startTime) + JudgeManager.Instance.goodWindow * 2f;
        }
        else
        {
            visualDuration = (endTime - startTime);
        }

        float length = visualDuration * ChartManager.Instance.CurrentScrollSpeed;

        bodyBase.localScale = new Vector3(bodyBase.localScale.x, length, 1f);
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, 0f, 1f);
        SetFillAlpha(1f);

        JudgeManager.Instance.RegisterHold(this);
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameStarted ||
            GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        Move();
        UpdateVisual();

        if (AutoPlayManager.Instance != null && AutoPlayManager.Instance.isAutoPlay)
        {
            HandleAutoPlay();
        }
        else
        {
            HandleMissCheck();
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver) return;

        // Despawn khi tail đã đi qua
        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float tailY = hitLineY + (endTime - songTime) * ChartManager.Instance.CurrentScrollSpeed;

        if (tailY < despawnY)
        {
            ObjectPoolingManager.Instance.ReturnHoldNote(gameObject);
        }
    }

    // ====================== STATE MACHINE ======================
    private void ChangeState(HoldState newState)
    {
        currentState = newState;
        // Có thể thêm OnEnterState logic nếu cần (ví dụ: particle, sound...)
    }

    void HandleAutoPlay()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (currentState == HoldState.Waiting && songTime >= startTime)
        {
            TryPress(); // auto perfect
        }

        if (currentState == HoldState.Holding && songTime >= endTime)
        {
            TryRelease(); // auto perfect
        }
    }

    void HandleMissCheck()
    {
  
        if (currentState == HoldState.Finished || currentState == HoldState.Missed)
            return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float goodWindow = JudgeManager.Instance.goodWindow;
        // Thêm vào đầu HandleMissCheck() hoặc tạo hàm riêng
        if (currentState == HoldState.Waiting &&
            songTime >= startTime - JudgeManager.Instance.goodWindow)
        {
            ChangeState(HoldState.HeadPending);
        }
        switch (currentState)
        {
            case HoldState.Waiting:
            case HoldState.HeadPending:
                // Miss head quá muộn
                if (songTime - startTime > 0.3f)
                {
                    MissHead();
                }
                // Chưa press mà đã quá thời gian tail
                else if (songTime > endTime + goodWindow)
                {
                    MissTail();
                }
                break;

            case HoldState.Holding:
                // Đang giữ mà quá hạn tail
                if (songTime > endTime + goodWindow)
                {
                    MissTail();
                }
                break;
        }
    }

    // ====================== INPUT ======================
    public void TryPress()
    {
        if (currentState != HoldState.Waiting && currentState != HoldState.HeadPending)
            return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float diff = Mathf.Abs(songTime - startTime);
        string result = JudgeManager.Instance.GetJudgeResult(diff);

        GameManager.Instance.ProcessJudgement(result);

        if (result == "MISS")
        {
            ChangeState(HoldState.Finished);
            return;
        }

        // SUCCESS → bắt đầu hold
        ChangeState(HoldState.Holding);
        holdStartTime = songTime;
        SnapFillToHitLine();
    }

    public void TryRelease()
    {
        if (currentState != HoldState.Holding) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float goodWindow = JudgeManager.Instance.goodWindow;

        // Thả quá sớm
        if (songTime < endTime - goodWindow)
        {
            GameManager.Instance.ProcessJudgement("MISS");
            ChangeState(HoldState.Finished);
            JudgeManager.Instance.RemoveHold(laneIndex);
            return;
        }

        // Release đúng
        float diff = Mathf.Abs(songTime - endTime);
        string result = JudgeManager.Instance.GetJudgeResult(diff);

        GameManager.Instance.ProcessJudgement(result);
        JudgeManager.Instance.RemoveHold(laneIndex);

        ChangeState(HoldState.Finished);

        if (result != "MISS")
        {
            isSuccess = true;
            FillFull();
            SetFillAlpha(0.3f); // fade nhẹ
        }
    }

    // ====================== VISUAL ======================
    void UpdateVisual()
    {
        float length = GetCurrentLength();

        bodyBase.localScale = new Vector3(bodyBase.localScale.x, length, 1f);

        switch (currentState)
        {
            case HoldState.Waiting:
            case HoldState.HeadPending:
                bodyFill.localScale = new Vector3(bodyFill.localScale.x, 0f, 1f);
                return;

            case HoldState.Finished when !isSuccess:
                // Giữ nguyên fill lúc miss
                bodyFill.localScale = new Vector3(bodyFill.localScale.x, currentFillLength, 1f);
                return;

            case HoldState.Finished when isSuccess:
                bodyFill.localScale = new Vector3(bodyFill.localScale.x, length, 1f);
                return;

            case HoldState.Holding:

                float songTime = (float)AudioManager.Instance.SongTimeDSP;

                float visualStart;
                float visualEnd;

                if (modePlay == ModePlay.VisualByJudge)
                {
                    float window = JudgeManager.Instance.goodWindow;
                    visualStart = startTime - window;
                    visualEnd = endTime + window;
                }
                else
                {
                    visualStart = startTime;
                    visualEnd = endTime;
                }

                float progress = Mathf.Clamp01((songTime - visualStart) / (visualEnd - visualStart));

                currentFillLength = length * progress;

                bodyFill.localScale = new Vector3(bodyFill.localScale.x, currentFillLength, 1f);
                break;
        }
    }

    float GetCurrentLength()
    {
        float duration;

        if (modePlay == ModePlay.VisualByJudge)
        {
            duration = (endTime - startTime) + JudgeManager.Instance.goodWindow * 2f;
        }
        else
        {
            duration = (endTime - startTime);
        }

        return duration * ChartManager.Instance.CurrentScrollSpeed;
    }

    void FillFull()
    {
        float length = GetCurrentLength();
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, length, 1f);
    }

    void SnapFillToHitLine()
    {
        // Giống logic cũ
        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float visualStart;
        float visualEnd;

        if (modePlay == ModePlay.VisualByJudge)
        {
            float window = JudgeManager.Instance.goodWindow;
            visualStart = startTime - window;
            visualEnd = endTime + window;
        }
        else
        {
            visualStart = startTime;
            visualEnd = endTime;
        }
        float progress = Mathf.Clamp01((songTime - visualStart) / (visualEnd - visualStart));
        currentFillLength = GetCurrentLength() * progress;
        bodyFill.localScale = new Vector3(bodyFill.localScale.x, currentFillLength, 1f);
    }

    void SetFillAlpha(float a)
    {
        if (fillSR == null) return;
        Color c = fillSR.color;
        c.a = a;
        fillSR.color = c;
    }

    // ====================== MOVE ======================
    void Move()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        float visualStartTime;

        if (modePlay == ModePlay.VisualByJudge)
        {
            visualStartTime = startTime - JudgeManager.Instance.goodWindow;
        }
        else
        {
            visualStartTime = startTime;
        }
        float timeUntilHit = visualStartTime - songTime;
        float yPosition = hitLineY + timeUntilHit * ChartManager.Instance.CurrentScrollSpeed;
        transform.position = new Vector3(spawnX, yPosition, 0f);
    }

    // ====================== MISS HELPERS ======================
    private void MissHead()
    {
        GameManager.Instance.ProcessJudgement("MISS");
        ChangeState(HoldState.Missed);
        JudgeManager.Instance.RemoveHold(laneIndex);
    }

    private void MissTail()
    {
        GameManager.Instance.ProcessJudgement("MISS");
        ChangeState(HoldState.Finished);
        JudgeManager.Instance.RemoveHold(laneIndex);
    }

    public void ForceMissTail() // nếu cần gọi từ ngoài
    {
        if (currentState != HoldState.Finished && currentState != HoldState.Missed)
        {
            MissTail();
            ObjectPoolingManager.Instance.ReturnHoldNote(gameObject);
        }
    }

    // Biến cũ bạn có thể xóa dần: headJudged, isHolding, finished → thay bằng currentState
}