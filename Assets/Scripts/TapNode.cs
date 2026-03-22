using UnityEngine;

public class TapNote : MonoBehaviour
{
    [Header("Note Data")]
    public int laneIndex;
    public float hitTime;
    private SpriteRenderer sr;
    [Header("Movement")]
    public float scrollSpeed;
    public float hitLineY = -3.5f;
    public float despawnY = -6.5f;   // khi ra khỏi màn hình

    private bool judged = false;
    private float spawnX;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void Initialize(int lane, float hit, float speed, float lineY)
    {
        laneIndex = lane;
        hitTime = hit;
        scrollSpeed = speed;
        hitLineY = lineY;

        judged = false;
        spawnX = transform.position.x;
        SetAlpha(1f);
        JudgeManager.Instance.RegisterNote(this);
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
        if (!GameManager.Instance.IsGameStarted) return; // 🔥 thêm dòng này

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
        float y = hitLineY + timeUntilHit * scrollSpeed;

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