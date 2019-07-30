using System.Collections.Generic;
using UnityEngine;

public class GameObjectTilemap
{
    private Dictionary<Tile, List<GameObject>> tileToBuildings;

    public GameObjectTilemap()
    {
        tileToBuildings = new Dictionary<Tile, List<GameObject>>(new TileEqualityComparer());
    }

    public void RemoveTile(Tile tile)
    {
        if (!tileToBuildings.ContainsKey(tile))
            return;

        List<GameObject> buildings = tileToBuildings[tile];

        foreach (var building in buildings)
        {
            Object.Destroy(building);
        }

        buildings.Clear();
        tileToBuildings.Remove(tile);
    }

    public void AttachObjectToTile(Tile tile, GameObject building)
    {
        if (!tileToBuildings.ContainsKey(tile))
        {
            tileToBuildings.Add(tile, new List<GameObject>());
        }

        tileToBuildings[tile].Add(building);
    }
}
