using System.Collections.Generic;
using UnityEngine;

public class VisualizedTile
{
    public Tile Tile;

    public Queue<GameObject> InstantiatedObjects;    

    public VisualizedTile(Tile tile)
    {
        Tile = tile;
        InstantiatedObjects = new Queue<GameObject>();
    }
}
