using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager Instance;
    [Header("Prefabs")]
    public GameObject tapNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject slideNotePrefab;

    [Header("Pool Size")]
    public int tapPoolSize = 200;
    public int holdPoolSize = 100;
    public int slidePoolSize = 50;
    [Header("Slide Point Prefabs")]
    public GameObject slideStartPrefab;
    public GameObject slideCheckPrefab;
    public GameObject slideEndPrefab;

    [Header("Slide Point Pool Size")]
    public int slideStartPoolSize = 100;
    public int slideCheckPoolSize = 300;
    public int slideEndPoolSize = 100;
    private Queue<GameObject> tapPool = new Queue<GameObject>();
    private Queue<GameObject> holdPool = new Queue<GameObject>();
    private Queue<GameObject> slidePool = new Queue<GameObject>();
    private Queue<GameObject> slideStartPool = new Queue<GameObject>();
    private Queue<GameObject> slideCheckPool = new Queue<GameObject>();
    private Queue<GameObject> slideEndPool = new Queue<GameObject>();
    public int activeNoteCount = 0;
    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        // SLIDE START
        for (int i = 0; i < slideStartPoolSize; i++)
        {
            GameObject obj = Instantiate(slideStartPrefab);
            obj.SetActive(false);
            slideStartPool.Enqueue(obj);
        }

        // SLIDE CHECK
        for (int i = 0; i < slideCheckPoolSize; i++)
        {
            GameObject obj = Instantiate(slideCheckPrefab);
            obj.SetActive(false);
            slideCheckPool.Enqueue(obj);
        }

        // SLIDE END
        for (int i = 0; i < slideEndPoolSize; i++)
        {
            GameObject obj = Instantiate(slideEndPrefab);
            obj.SetActive(false);
            slideEndPool.Enqueue(obj);
        }
        // SLIDE
        for (int i = 0; i < slidePoolSize; i++)
        {
            GameObject obj = Instantiate(slideNotePrefab);
            obj.SetActive(false);
            slidePool.Enqueue(obj);
        }
        // TAP
        for (int i = 0; i < tapPoolSize; i++)
        {
            GameObject obj = Instantiate(tapNotePrefab);
            obj.SetActive(false);
            tapPool.Enqueue(obj);
        }

        // HOLD
        for (int i = 0; i < holdPoolSize; i++)
        {
            GameObject obj = Instantiate(holdNotePrefab);
            obj.SetActive(false);
            holdPool.Enqueue(obj);
        }
    }

    public GameObject GetTapNote()
    {
        GameObject obj;

        if (tapPool.Count > 0)
        {
            obj = tapPool.Dequeue();
        }
        else
        {
            obj = Instantiate(tapNotePrefab);
        }

        obj.SetActive(true);
        activeNoteCount++; // 🔥 ADD

        return obj;
    }

    public void ReturnTapNote(GameObject obj)
    {
        obj.SetActive(false);
        tapPool.Enqueue(obj);

        activeNoteCount--; // 🔥 REMOVE
    }

    public GameObject GetHoldNote()
    {
        GameObject obj;

        if (holdPool.Count > 0)
        {
            obj = holdPool.Dequeue();
        }
        else
        {
            obj = Instantiate(holdNotePrefab);
        }

        obj.SetActive(true);
        activeNoteCount++; // 🔥 ADD

        return obj;
    }

    public void ReturnHoldNote(GameObject obj)
    {
        obj.SetActive(false);
        holdPool.Enqueue(obj);

        activeNoteCount--; // 🔥 REMOVE
    }
    public GameObject GetSlideNote()
    {
        GameObject obj;

        if (slidePool.Count > 0)
        {
            obj = slidePool.Dequeue();
        }
        else
        {
            obj = Instantiate(slideNotePrefab);
        }

        obj.SetActive(true);
        activeNoteCount++; // 🔥 ADD

        return obj;
    }
    public void ReturnSlideNote(GameObject obj)
    {
        obj.SetActive(false);
        slidePool.Enqueue(obj);

        activeNoteCount--; // 🔥 REMOVE
    }
    public GameObject GetSlideStart()
    {
        if (slideStartPool.Count > 0)
        {
            GameObject obj = slideStartPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(slideStartPrefab);
    }

    public GameObject GetSlideCheck()
    {
        if (slideCheckPool.Count > 0)
        {
            GameObject obj = slideCheckPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(slideCheckPrefab);
    }

    public GameObject GetSlideEnd()
    {
        if (slideEndPool.Count > 0)
        {
            GameObject obj = slideEndPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(slideEndPrefab);
    }
    public void ReturnSlidePoint(GameObject obj, SlidePoint point)
    {
        obj.SetActive(false);

        if (point.isEndPoint)
        {
            slideEndPool.Enqueue(obj);
        }
        else
        {
            slideCheckPool.Enqueue(obj);
        }
    }
}