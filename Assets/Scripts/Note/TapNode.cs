using UnityEngine;

public class TapNote : MonoBehaviour
{
    [Header("Note Data")]
    public int laneIndex;
    public float hitTime;
    private SpriteRenderer sr;
    [Header("Movement")]
    [Header("Scale Visual")]
    [SerializeField] private float minScaleY = 0.5f;
    [SerializeField] private float maxScaleY = 1.5f;
    private float baseScaleY = 1f;
    public float hitLineY = -3.5f;
    public float despawnY = -6.5f;   // khi ra khỏi màn hình

    private bool judged = false;
    private float spawnX;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void Initialize(int lane, float hit,  float lineY)
    {
        laneIndex = lane;
        hitTime = hit;
        hitLineY = lineY;

        judged = false;
        spawnX = transform.position.x;
        SetAlpha(1f);
        JudgeManager.Instance.RegisterNote(this);
        baseScaleY = transform.localScale.y;
        UpdateScaleByJudge();
    }
    void UpdateScaleByJudge()
    {
        float window = JudgeManager.Instance.goodWindow;

        float speed = ChartManager.Instance.CurrentScrollSpeed;

        float length = window * 2f * speed;

        transform.localScale = new Vector3(
            transform.localScale.x,
            length,
            transform.localScale.z
        );
    }
    void SetAlpha(float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

    }
    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver) return; // 🔥 THÊM

        Move();

        if (AutoPlayManager.Instance != null &&
            AutoPlayManager.Instance.isAutoPlay)
        {
            AutoJudge();
        }
        else
        {
            CheckMiss();
        }
        UpdateScaleByJudge();
        CheckDespawn();
      
    }
    void AutoJudge()
    {
        if (judged) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (songTime >= hitTime)
        {
            // 👇 dùng lại hệ thống chung
            JudgeHit("PERFECT");
        }
    }
    void Move()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float timeUntilHit = hitTime - songTime;

        float currentSpeed = ChartManager.Instance.CurrentScrollSpeed;

        float y = hitLineY + timeUntilHit * currentSpeed;

        // 🔥 offset theo scale để giữ tâm đúng
        float halfLength = transform.localScale.y / 2f;

        transform.position = new Vector3(spawnX, y, 0f);
    }

    void CheckMiss()
    {
        if (judged) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (songTime - hitTime > JudgeManager.Instance.goodWindow)
        {
            judged = true;

            GameManager.Instance.ProcessJudgement("MISS");
            JudgeManager.Instance.RemoveTap(laneIndex);
        }
    }

    void CheckDespawn()
    {
        if (transform.position.y < despawnY)
        {
            ObjectPoolingManager.Instance.ReturnTapNote(gameObject);
        }
    }

    public void JudgeHit(string result)
    {
        if (judged) return;

        judged = true;

        // 👇 CHỈ giảm alpha nếu KHÔNG phải MISS
        if (result != "MISS")
        {
            SetAlpha(0.3f);
        }

        GameManager.Instance.ProcessJudgement(result);
        JudgeManager.Instance.RemoveTap(laneIndex);
    }
}