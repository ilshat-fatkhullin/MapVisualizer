using UnityEngine;

public class OsmNode
{
    public Vector2 Coordinate { get; private set; }

    public OsmNode(Vector2 coordinate)
    {
        Coordinate = coordinate;
    }
}
