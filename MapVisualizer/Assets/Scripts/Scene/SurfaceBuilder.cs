using System.Collections.Generic;
using UnityEngine;

public class SurfaceBuilder
{
    public Material Material { get; private set; }

    public float Tiling { get; private set; }

    private List<Vector3> vertices;    

    private List<int> triangles;

    public SurfaceBuilder(Material material, float tiling)
    {
        Material = material;
        Tiling = tiling;
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    public void AddCell(Point2D cell)
    {
        vertices.Add(new Vector3(cell.X, 0, cell.Y));
        vertices.Add(new Vector3(cell.X, 0, cell.Y + 1));
        vertices.Add(new Vector3(cell.X + 1, 0, cell.Y + 1));
        vertices.Add(new Vector3(cell.X + 1, 0, cell.Y));

        

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 4);
    }

    public bool IsEmpty()
    {
        return vertices.Count == 0 || triangles.Count == 0;
    }

    public GameObject Instantiate(Vector3 position)
    {        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        Vector2[] uv = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x * Tiling, vertices[i].z * Tiling);
        }

        mesh.uv = uv;
        mesh.RecalculateNormals();

        var surface = Object.Instantiate(SurfaceVisualizer.Instance.SurfacePrefab, position, Quaternion.identity);
        surface.GetComponent<MeshFilter>().mesh = mesh;
        surface.GetComponent<MeshRenderer>().material = Material;

        return surface;
    }
}
