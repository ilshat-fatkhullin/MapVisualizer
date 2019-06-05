using System.Collections.Generic;
using UnityEngine;

public class VisualizedTile
{
    public Tile Tile;

    public List<GameObject> InstantiatedObjects;    

    public VisualizedTile(Tile tile)
    {
        Tile = tile;
        InstantiatedObjects = new List<GameObject>();
    }
}
