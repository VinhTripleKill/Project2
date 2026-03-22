using UnityEngine;

public class ChartManager : MonoBehaviour
{
    public TextAsset chartFile;
    public GameObject tapNotePrefab;
    public Transform[] laneSpawnPoints;
    public float hitLineY = -3.5f;
    public float spawnY = 5.5f;
    public float spawnLeadTime = 2f;
    public GameObject holdNotePrefab;
    private ChartData chartData;
    private int nextNoteIndex = 0;
    public GameObject slideNotePrefab;
    private float scrollSpeed;

    void Start()
    {
        chartData = JsonUtility.FromJson<ChartData>(chartFile.text);

        if (chartData == null || chartData.notes == null)
        {
            Debug.LogError("Chart JSON parse failed!");
            return;
        }

        float distance = spawnY - hitLineY;
        scrollSpeed = distance / spawnLeadTime;
    }
    void Update()
    {
        if (!GameManager.Instance.IsGameStarted) return; // 🔥 chặn từ đầu

        if (AudioManager.Instance == null) return;

        SpawnNotesByTime();
    }

    void SpawnNotesByTime()
    {
        if (nextNoteIndex >= chartData.notes.Length)
            return;

        double songTime = AudioManager.Instance.SongTimeDSP;

        while (nextNoteIndex < chartData.notes.Length &&
               chartData.notes[nextNoteIndex].hitTime - songTime <= spawnLeadTime)
        {
            SpawnNote(chartData.notes[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    void SpawnNote(NoteData noteData)
    {
        if (noteData.slideNodes != null && noteData.slideNodes.Length > 1)
        {
            GameObject obj = ObjectPoolingManager.Instance.GetSlideNote();
            SlideNote slide = obj.GetComponent<SlideNote>();

            slide.laneSpawnPoints = laneSpawnPoints;
            slide.scrollSpeed = scrollSpeed;
            slide.hitLineY = hitLineY;

            slide.Initialize(noteData.slideNodes);

            return;
        }

        // TAP
        if (noteData.endTime <= noteData.hitTime)
        {
            Transform spawn = laneSpawnPoints[noteData.lane];
            GameObject obj = ObjectPoolingManager.Instance.GetTapNote();
            obj.transform.position = spawn.position;
            TapNote tap = obj.GetComponent<TapNote>();
            tap.Initialize(noteData.lane, noteData.hitTime, scrollSpeed, hitLineY);
        }

        // HOLD
        else
        {
            Transform spawn = laneSpawnPoints[noteData.lane];
            GameObject obj = ObjectPoolingManager.Instance.GetHoldNote();
            obj.transform.position = spawn.position;
            HoldNote hold = obj.GetComponent<HoldNote>();

            hold.Initialize(noteData.lane, noteData.hitTime, noteData.endTime, scrollSpeed, hitLineY);
        }
    }
    
}