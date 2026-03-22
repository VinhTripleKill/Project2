using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class SmoothLine : MonoBehaviour
{
    public int minSections = 3;
    public int maxSections = 12;
    public float maxAngle = 90f;

    private LineRenderer line;
    private SlideNote slide;

    // cache
    private List<Vector3> smoothPoints = new List<Vector3>(128);
    private Vector3[] lineBuffer = new Vector3[128];

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        slide = GetComponent<SlideNote>();
    }

    void LateUpdate()
    {
        var nodes = slide.GetNodes();

        if (nodes.Count < 2)
            return;

        GenerateSmoothLine(nodes);

        // expand buffer nếu cần
        if (lineBuffer.Length < smoothPoints.Count)
        {
            lineBuffer = new Vector3[smoothPoints.Count];
        }

        for (int i = 0; i < smoothPoints.Count; i++)
            lineBuffer[i] = smoothPoints[i];

        line.positionCount = smoothPoints.Count;
        line.SetPositions(lineBuffer);
    }

    void GenerateSmoothLine(List<SlidePoint> nodes)
    {
        smoothPoints.Clear();

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? nodes[i].transform.position : nodes[i - 1].transform.position;
            Vector3 p1 = nodes[i].transform.position;
            Vector3 p2 = nodes[i + 1].transform.position;
            Vector3 p3 = i + 2 < nodes.Count ? nodes[i + 2].transform.position : p2;

            int sections = CalculateAdaptiveSections(p0, p1, p2, p3);

            for (int j = 0; j < sections; j++)
            {
                float t = j / (float)sections;
                smoothPoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        smoothPoints.Add(nodes[nodes.Count - 1].transform.position);
    }

    int CalculateAdaptiveSections(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 dir1 = (p2 - p1).normalized;
        Vector3 dir2 = (p3 - p2).normalized;

        float angle = Vector3.Angle(dir1, dir2);
        float t = Mathf.Clamp01(angle / maxAngle);

        return Mathf.RoundToInt(Mathf.Lerp(minSections, maxSections, t));
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}