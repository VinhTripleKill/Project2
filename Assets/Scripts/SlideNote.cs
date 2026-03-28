using System.Collections.Generic;
using UnityEngine;

public class SlideNote : MonoBehaviour
{
    public GameObject startPrefab;
    public GameObject checkPrefab;
    public GameObject endPrefab;

    private SlidePoint endNode;
    public Transform[] laneSpawnPoints;

    private SmoothLine smoothLine;
    private List<SlidePoint> nodes = new List<SlidePoint>();

    public List<SlidePoint> GetNodes() { return nodes; }

    public float scrollSpeed;
    public float hitLineY;

    private int remainingNodes = 0;

    void Awake()
    {
        smoothLine = GetComponent<SmoothLine>();
    }

    public void Initialize(SlideNode[] slideNodesData)
    {
        ResetNodes();

        if (slideNodesData == null || slideNodesData.Length < 2)
        {
            Debug.LogWarning("[SlideNote] Invalid slideNodesData (null or < 2)");
            return;
        }

        // ✅ COPY để không phá data gốc
        SlideNode[] sortedNodes = (SlideNode[])slideNodesData.Clone();

        // ✅ SORT theo time
        System.Array.Sort(sortedNodes, (a, b) => a.time.CompareTo(b.time));

        // ===== SPAWN =====
        for (int i = 0; i < sortedNodes.Length; i++)
        {
            SlideNode data = sortedNodes[i];

            GameObject obj;
            if (i == 0)
                obj = ObjectPoolingManager.Instance.GetSlideStart();
            else if (i == sortedNodes.Length - 1)
                obj = ObjectPoolingManager.Instance.GetSlideEnd();
            else
                obj = ObjectPoolingManager.Instance.GetSlideCheck();

            Transform spawn = laneSpawnPoints[data.lane];
            obj.transform.SetParent(transform);
            obj.transform.position = spawn.position;

            SlidePoint node = obj.GetComponent<SlidePoint>();
            node.Initialize(data.lane, data.time, scrollSpeed, hitLineY, this);

            nodes.Add(node);

            if (i == sortedNodes.Length - 1)
            {
                node.isEndPoint = true;
                endNode = node;
            }
        }

        remainingNodes = nodes.Count;
    }

    void ResetNodes()
    {
        foreach (var node in nodes)
        {
            ObjectPoolingManager.Instance.ReturnSlidePoint(node.gameObject, node);
        }
        nodes.Clear();
    }

    void OnDisable()
    {
        nodes.Clear();
    }

    public void NotifyNodeHit(SlidePoint node)
    {
        int index = nodes.IndexOf(node);

        // Chỉ xử lý start point (index == 0)
        if (index == 0)
        {
            // Không còn fill nữa → không cần làm gì đặc biệt
        }
    }

    public void NotifyNodeMiss(SlidePoint node)
    {
        int index = nodes.IndexOf(node);

        // Chỉ xử lý start point miss
        if (index == 0)
        {
            // Không còn fill → không cần set canFill
        }

        // Lock các node tiếp theo (vẫn giữ logic này)
        for (int i = index + 1; i < nodes.Count; i++)
        {
            nodes[i].SetState(SlidePoint.SlidePointState.Lock);
        }
    }

    public void NotifyEndReached()
    {
        ObjectPoolingManager.Instance.ReturnSlideNote(gameObject);
    }

    public void NotifyNodeDestroyed()
    {
        remainingNodes--;
        if (remainingNodes <= 0)
        {
            ObjectPoolingManager.Instance.ReturnSlideNote(gameObject);
        }
    }
}