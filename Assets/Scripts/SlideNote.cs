using System.Collections.Generic;
using UnityEngine;

public class SlideNote : MonoBehaviour
{
    public GameObject startPrefab;
    public GameObject checkPrefab;
    public GameObject endPrefab;
    private SlidePoint endNode;
    public Transform[] laneSpawnPoints;
    private List<SlidePoint> nodes = new List<SlidePoint>();
    public List<SlidePoint> GetNodes()
    {
        return nodes;
    }
    public float scrollSpeed;
    public float hitLineY;

    private int remainingNodes = 0;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Initialize(SlideNode[] slideNodesData)
    {
        ResetNodes();

        for (int i = 0; i < slideNodesData.Length; i++)
        {
            GameObject prefab;

            if (i == 0)
                prefab = startPrefab;
            else if (i == slideNodesData.Length - 1)
                prefab = endPrefab;
            else
                prefab = checkPrefab;

            Transform spawn = laneSpawnPoints[slideNodesData[i].lane];
            GameObject obj;

            if (i == 0)
                obj = ObjectPoolingManager.Instance.GetSlideStart();
            else if (i == slideNodesData.Length - 1)
                obj = ObjectPoolingManager.Instance.GetSlideEnd();
            else
                obj = ObjectPoolingManager.Instance.GetSlideCheck();

            obj.transform.SetParent(transform);
            obj.transform.position = spawn.position;
            SlidePoint node = obj.GetComponent<SlidePoint>();

            node.Initialize(
            slideNodesData[i].lane,
            slideNodesData[i].time,
            scrollSpeed,hitLineY,this
             );

            nodes.Add(node);

            if (i == slideNodesData.Length - 1)
            {
                node.isEndPoint = true;
                endNode = node;
            }
        }

        line.positionCount = nodes.Count;
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

        if (index + 1 < nodes.Count)
        {
            SlidePoint next = nodes[index + 1];

            if (next.state == SlidePoint.SlidePointState.Lock)
                return;
        }
    }
    public void NotifyNodeMiss(SlidePoint node)
    {
        int index = nodes.IndexOf(node);

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