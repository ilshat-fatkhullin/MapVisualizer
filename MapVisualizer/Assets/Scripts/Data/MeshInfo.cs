using UnityEngine;

public class MeshInfo
{
    public int[] Triangles { get; private set; }

    public Vector3[] Vertices { get; private set; }

    public Vector3[] UVs { get; private set; }

    public MeshInfo(Vector3[] vertices, int[] triangles, Vector3[] uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }
}
