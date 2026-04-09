using UnityEngine;

public class SlidePoint : MonoBehaviour
{
    public int laneIndex;
    public float hitTime;
    public float hitLineY;
    public bool isEndPoint = false;
    public float despawnY = -9.5f;
    public SlideNote parentSlide;
    private bool judged = false;
    private float spawnX;
    public enum SlidePointState
    {
        Normal,
        Accurate,
        Miss,
        Lock
    }
    public Sprite lockSprite;
    public Sprite accurateSprite;
    public Sprite missSprite;

    private SpriteRenderer sr;
    public SlidePointState state = SlidePointState.Normal;
    public void Initialize(int lane, float time, float lineY, SlideNote parent)
    {
        laneIndex = lane;
        hitTime = time;
        hitLineY = lineY;
        parentSlide = parent;

        judged = false;
        state = SlidePointState.Normal;

        spawnX = transform.position.x;

        sr = GetComponent<SpriteRenderer>();

        JudgeManager.Instance.RegisterSlideNode(this);
    }
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        spawnX = transform.position.x;

        
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return;
        if (GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver) return; // 🔥

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
        if (judged || state == SlidePointState.Lock)
            return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (songTime >= hitTime)
        {
            judged = true;

            SetState(SlidePointState.Accurate);

            GameManager.Instance.ProcessJudgement("PERFECT");

            JudgeManager.Instance.RemoveSlideNode(laneIndex);

            if (parentSlide != null)
                parentSlide.NotifyNodeHit(this);
        }
    }

    void Move()
    {
        float songTime = (float)AudioManager.Instance.SongTimeDSP;
        float timeUntilHit = hitTime - songTime;
        float speed = ChartManager.Instance.CurrentScrollSpeed;
        float y = hitLineY + timeUntilHit * speed;
        transform.position = new Vector3(spawnX, y, 0);
    }
    public void SetState(SlidePointState newState)
    {
        state = newState;

        switch (state)
        {
            case SlidePointState.Accurate:
                sr.sprite = accurateSprite;
                break;

            case SlidePointState.Miss:
                sr.sprite = missSprite;
                break;

            case SlidePointState.Lock:
                sr.sprite = lockSprite;
                break;
        }
    }
    void CheckMiss()
    {
        if (judged) return;

        float songTime = (float)AudioManager.Instance.SongTimeDSP;

        if (songTime - hitTime > JudgeManager.Instance.goodWindow)
        {
            judged = true;

            SetState(SlidePointState.Miss);

            GameManager.Instance.ProcessJudgement("MISS");

            JudgeManager.Instance.RemoveSlideNode(laneIndex);

            if (parentSlide != null)
                parentSlide.NotifyNodeMiss(this);
        }
    }
    void CheckDespawn()
    {
        if (GameManager.Instance.IsPaused) return; // 🔥 THÊM

        if (transform.position.y < despawnY)
        {
            if (isEndPoint && parentSlide != null)
            {
                parentSlide.NotifyEndReached();
            }
        }
    }
    public void JudgeHit(string result)
    {
        if (judged || state == SlidePointState.Lock) return;

        judged = true;

        SetState(SlidePointState.Accurate);

        GameManager.Instance.ProcessJudgement(result);

        JudgeManager.Instance.RemoveSlideNode(laneIndex);

        if (parentSlide != null)
            parentSlide.NotifyNodeHit(this);
    }
    public bool IsLocked()
    {
        return state == SlidePointState.Lock;
    }
}