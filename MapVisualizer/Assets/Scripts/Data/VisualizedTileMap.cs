using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualizedTileMap
{
    public Queue<GameObject> ObjectsToDestroy;

    public Queue<ObjectToInstantiate> ObjectsToInstantiate;

    public List<Tile> TilesToVisualize;

    private VisualizedTile[,] map;

    public VisualizedTileMap(Tile centerTile)
    {
        ObjectsToDestroy = new Queue<GameObject>();
        ObjectsToInstantiate = new Queue<ObjectToInstantiate>();
        TilesToVisualize = new List<Tile>();
        map = GetMap(centerTile);
    }

    private static VisualizedTile[,] GetMap(Tile centerTile)
    {
        VisualizedTile[,] map = new VisualizedTile[3, 3];
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                map[i + 1, j + 1] = new VisualizedTile(new Tile(centerTile.X + i, centerTile.Y + j, centerTile.Zoom));
            }
        return map;
    }

    public void UpdateCenterTile(Tile centerTile)
    {
        VisualizedTile[,] newMap = GetMap(centerTile);

        //Adding objects that now are out of our map into <code>ObjectsToDestroy</code> list.

        for (int x1 = 0; x1 < map.GetLength(0); x1++)
            for (int y1 = 0; y1 < map.GetLength(1); y1++)
            {
                bool doesNotExist = true;

                for (int x2 = 0; x2 < newMap.GetLength(0); x2++)
                    for (int y2 = 0; y2 < newMap.GetLength(1); y2++)
                    {
                        if (newMap[x2, y2].Tile == map[x1, y1].Tile)
                        {                            
                            doesNotExist = false;
                            break;
                        }
                    }

                if (doesNotExist)
                {
                    if (map[x1, y1].InstantiatedObjects.Count > 0)
                    {                        
                        ObjectsToDestroy = new Queue<GameObject>(ObjectsToDestroy.Concat(map[x1, y1].InstantiatedObjects));
                    }
                }
            }

        //Adding tiles that are new and not rendered yet into <code>TilesToVisualize</code> list.

        for (int x1 = 0; x1 < newMap.GetLength(0); x1++)
            for (int y1 = 0; y1 < newMap.GetLength(1); y1++)
            {
                bool doesNotExist = true;

                for (int x2 = 0; x2 < map.GetLength(0); x2++)
                    for (int y2 = 0; y2 < map.GetLength(1); y2++)
                    {
                        if (map[x2, y2].Tile == newMap[x1, y1].Tile)
                        {
                            doesNotExist = false;
                            newMap[x1, y1] = map[x2, y2];
                            break;
                        }
                    }

                if (doesNotExist)
                {
                    TilesToVisualize.Add(newMap[x1, y1].Tile);
                }
            }

        map = newMap;
    }

    public bool ContainsTile(Tile tile)
    {
        for (int i = 0; i < map.GetLength(0); i++)
            for (int j = 0; j < map.GetLength(0); j++)
            {
                if (map[i, j].Tile == tile)
                {
                    return true;
                }
            }
        return false;
    }

    public void AddInstantiatedObjectToTile(Tile tile, GameObject gameObject)
    {
        for (int i = 0; i < map.GetLength(0); i++)
            for (int j = 0; j < map.GetLength(0); j++)
            {
                if (map[i, j].Tile == tile)
                {
                    map[i, j].InstantiatedObjects.Enqueue(gameObject);
                    return;
                }
            }
        throw new System.Exception("Tile is out of map.");
    }

    public void AddObjectToInstantitate(ObjectToInstantiate objectToInstantiate)
    {
        ObjectsToInstantiate.Enqueue(objectToInstantiate);
    }
}
