using UnityEngine;
using System;
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

        // ✅ VALIDATE TRƯỚC
        ValidateChart();

        // ✅ SORT NOTE
        Array.Sort(chartData.notes, CompareNoteTime);

        float distance = spawnY - hitLineY;
        scrollSpeed = distance / spawnLeadTime;
    }
    void ValidateChart()
    {
        foreach (var note in chartData.notes)
        {
            // ===== HOLD CHECK =====
            if (note.endTime > note.hitTime)
            {
                // OK hold
            }
            else if (note.endTime < note.hitTime)
            {
                Debug.LogError($"[Chart ERROR] HoldNote endTime < hitTime | lane={note.lane} | hit={note.hitTime} | end={note.endTime}");
            }

            // ===== SLIDE CHECK =====
            if (note.slideNodes != null && note.slideNodes.Length > 1)
            {
                for (int i = 0; i < note.slideNodes.Length - 1; i++)
                {
                    if (note.slideNodes[i].time > note.slideNodes[i + 1].time)
                    {
                        Debug.LogWarning($"[Chart Warning] SlideNodes not ordered at index {i}");
                        break;
                    }
                }
            }

        }
    }
    float GetNoteTime(NoteData note)
    {
        // Slide → lấy node đầu tiên
        if (note.slideNodes != null && note.slideNodes.Length > 0)
        {
            return note.slideNodes[0].time;
        }

        // Tap / Hold
        return note.hitTime;
    }
    int CompareNoteTime(NoteData a, NoteData b)
    {
        float timeA = GetNoteTime(a);
        float timeB = GetNoteTime(b);

        return timeA.CompareTo(timeB);
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
               GetNoteTime(chartData.notes[nextNoteIndex]) - songTime <= spawnLeadTime)
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
    public void LoadChartFromSong(TextAsset newChart)
    {
        chartFile = newChart;

        chartData = JsonUtility.FromJson<ChartData>(chartFile.text);

        if (chartData == null || chartData.notes == null)
        {
            Debug.LogError("Chart JSON parse failed!");
            return;
        }

        ValidateChart();
        Array.Sort(chartData.notes, CompareNoteTime);

        float distance = spawnY - hitLineY;
        baseScrollSpeed = distance / spawnLeadTime;
        scrollSpeed = baseScrollSpeed;

        nextNoteIndex = 0;     // ⚠ reset index
        stopSpawn = false;     // ⚠ reset state
        notifiedEnd = false;   // ⚠ reset end flag

        CalculateMaxPerfectScore();
    }
}