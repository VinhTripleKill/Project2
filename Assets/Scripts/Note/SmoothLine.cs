using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SmoothLine : MonoBehaviour
{
    [SerializeField] private Color slide_BaseS = Color.cyan;

    public int minSections = 3;
    public int maxSections = 12;
    public float maxAngle = 90f;
    public float width = 0.5f;

    private Mesh mesh;
    private SlideNote slide;

    private List<Vector3> smoothPoints = new List<Vector3>(128);

    void Awake()
    {
        slide = GetComponent<SlideNote>();
        mesh = new Mesh();
        mesh.name = "SlideMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        var mr = GetComponent<MeshRenderer>();

        mr.sortingLayerName = "LineMesh";
        mr.sortingOrder = 0;
    }

    void LateUpdate()
    {
      
        if (GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver) return; // 🔥
        if (slide == null) return;

        var nodes = slide.GetNodes();
        if (nodes == null || nodes.Count < 2)
        {
            mesh.Clear();
            return;
        }

        GenerateSmoothLine(nodes);
        BuildMesh();
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

    // ================= MESH =================
    void BuildMesh()
    {
        int pointCount = smoothPoints.Count;
        if (pointCount < 2)
        {
            mesh.Clear();
            return;
        }

        Vector3[] vertices = new Vector3[pointCount * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(pointCount - 1) * 6];

        float halfW = width * 0.5f;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 p = transform.InverseTransformPoint(smoothPoints[i]);
            Vector3 right = Vector3.right * halfW;

            int v = i * 2;
            vertices[v] = p - right;
            vertices[v + 1] = p + right;

            float t = (float)i / (pointCount - 1);
            uvs[v] = new Vector2(0, t);
            uvs[v + 1] = new Vector2(1, t);
        }

        int tri = 0;
        for (int i = 0; i < pointCount - 1; i++)
        {
            int v = i * 2;
            triangles[tri++] = v;
            triangles[tri++] = v + 1;
            triangles[tri++] = v + 3;
            triangles[tri++] = v + 3;
            triangles[tri++] = v + 2;
            triangles[tri++] = v;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
    }
}