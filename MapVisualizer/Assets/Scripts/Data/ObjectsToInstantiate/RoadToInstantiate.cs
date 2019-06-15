using UnityEngine;

public class RoadToInstantiate: ObjectToInstantiate
{
    public Road Road { get; private set; }

    public RoadToInstantiate(Road road, Tile tile): base(tile)
    {
        Road = road;      
    }
}
