using UnityEngine;

public class MeshInfo
{
    public int[] Triangles { get; private set; }

    public Vector3[] Vertices { get; private set; }

    public MeshInfo(Vector3[] vertices, int[] triangles)
    {
        Vertices = vertices;
        Triangles = triangles;
    }
}
