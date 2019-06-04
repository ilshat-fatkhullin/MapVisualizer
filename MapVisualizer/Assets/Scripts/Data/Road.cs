using UnityEngine;

public class Road
{
    public int Lanes { get; private set; }

    public Vector2[] Points { get; private set; }

    public Road(int lanes, Vector2[] points)
    {
        Lanes = lanes;
        Points = points;
    }
}
