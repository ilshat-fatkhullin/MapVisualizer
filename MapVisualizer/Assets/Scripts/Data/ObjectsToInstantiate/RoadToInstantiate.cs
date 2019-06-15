using UnityEngine;

public class RoadToInstantiate: ObjectToInstantiate
{
    public Road Road { get; private set; }

    public Vector3[] Points { get; private set; }    

    public RoadToInstantiate(Road road, Vector3[] points, Tile tile): base(tile)
    {
        Road = road;
        Points = points;        
    }
}
