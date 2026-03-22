using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StretchBeamSmooth : MonoBehaviour
{
    public Transform posA;
    public Transform posB;
    public Transform[] checkpoints;

    public float width = 1f;

    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "SmoothBeam";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if (posA == null || posB == null) return;

        // ===== 1. Gom tất cả điểm =====
        List<Vector3> points = new List<Vector3>();
        points.Add(posA.position);

        if (checkpoints != null)
        {
            foreach (var cp in checkpoints)
            {
                if (cp != null)
                    points.Add(cp.position);
            }
        }

        points.Add(posB.position);

        int pointCount = points.Count;

        // ===== 2. Tạo vertices (2 mỗi điểm) =====
        Vector3[] vertices = new Vector3[pointCount * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(pointCount - 1) * 6];

        float halfW = width * 0.5f;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 p = points[i];

            // luôn giữ ngang
            Vector3 right = Vector3.right * halfW;

            int v = i * 2;

            vertices[v] = p - right;     // left
            vertices[v + 1] = p + right; // right

            // UV chạy xuyên suốt (quan trọng)
            float t = (float)i / (pointCount - 1);

            uvs[v] = new Vector2(0, t);
            uvs[v + 1] = new Vector2(1, t);
        }

        // ===== 3. Triangles nối liên tục =====
        int triIndex = 0;

        for (int i = 0; i < pointCount - 1; i++)
        {
            int v = i * 2;

            triangles[triIndex++] = v;
            triangles[triIndex++] = v + 1;
            triangles[triIndex++] = v + 3;

            triangles[triIndex++] = v + 3;
            triangles[triIndex++] = v + 2;
            triangles[triIndex++] = v;
        }

        // ===== 4. Apply =====
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
    }
}