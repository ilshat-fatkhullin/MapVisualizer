using UnityEngine;
using System.Collections;

public class Polygon2D
{

    public int[] triangles;
    public Vector2[] vertices;

    public Polygon2D(int[] triangle, Vector2[] vertices)
    {
        this.triangles = triangle;
        this.vertices = vertices;
    }
}