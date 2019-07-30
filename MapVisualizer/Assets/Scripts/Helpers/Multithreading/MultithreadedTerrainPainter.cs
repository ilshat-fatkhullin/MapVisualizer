using System;
using System.Collections.Generic;
using UnityEngine;

public class MultithreadedTerrainPainter: MultithreadedEntity
{
    public float[,,] Alphamap;

    public Tile Tile;

    private int alphamapWidth;   

    private int alphamapLayers;    

    private Vector2 tileSizeInMeters;    

    private PriorityQueue<Road> roadsQueue;

    private PriorityQueue<Area> areasQueue;

    private Dictionary<string, int> nameToTerrainLayer;

    public MultithreadedTerrainPainter(int alphamapWidth, int alphamapLayers,
        Tile tile, Vector2 tileSizeInMeters, PriorityQueue<Road> roadsQueue, PriorityQueue<Area> areasQueue,
        Dictionary<string, int> nameToTerrainLayer)
    {
        Tile = tile;
        this.alphamapWidth = alphamapWidth;
        this.alphamapLayers = alphamapLayers;
        this.tileSizeInMeters = tileSizeInMeters;
        this.roadsQueue = roadsQueue;
        this.areasQueue = areasQueue;
        this.nameToTerrainLayer = nameToTerrainLayer;
    }

    protected override void ExecuteThread()
    {
        Intmap surfaceIntmap = new Intmap(alphamapWidth, alphamapWidth);

        while (areasQueue.Count() > 0)
        {
            Area area = areasQueue.Dequeue();

            if (area.Nodes.Length < 3)
                continue;

            Point2D[] points = new Point2D[area.Nodes.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetTerrainMapPoint(area.Nodes[i]);
            }

            surfaceIntmap.DrawFilledPolygon(points, GetTerrainLayerByName(Enum.GetName(typeof(Area.AreaType), area.Type)));
        }

        while (roadsQueue.Count() > 0)
        {
            Road road = roadsQueue.Dequeue();

            if (road.Nodes.Length < 2)
                continue;

            for (int i = 1; i < road.Nodes.Length; i++)
            {
                Point2D point1 = GetTerrainMapPoint(road.Nodes[i - 1]);
                Point2D point2 = GetTerrainMapPoint(road.Nodes[i]);

                surfaceIntmap.DrawLine(point1, point2,
                                       GetTerrainLayerByName(Enum.GetName(typeof(Road.RoadType), road.Type)),
                                       MetersToTerrainMapCells(road.GetRoadWidth()));
            }
        }

        Alphamap = new float[alphamapWidth, alphamapWidth, alphamapLayers];

        for (int layer = 0; layer < Alphamap.GetLength(2); layer++)
            for (int x = 0; x < Alphamap.GetLength(0); x++)
                for (int y = 0; y < Alphamap.GetLength(1); y++)
                {
                    if (surfaceIntmap.Map[x, y] == layer)
                    {
                        Alphamap[y, x, layer] = 1;
                    }
                    else
                    {
                        Alphamap[y, x, layer] = 0;
                    }
                }
    }

    private Point2D GetTerrainMapPoint(Vector2 coordinate)
    {
        return new Point2D(Mathf.RoundToInt((coordinate.x / tileSizeInMeters.x) * alphamapWidth),
                           Mathf.RoundToInt((coordinate.y / tileSizeInMeters.y) * alphamapWidth));
    }

    private int MetersToTerrainMapCells(float meters)
    {
        return Mathf.RoundToInt(meters * alphamapWidth / tileSizeInMeters.x);
    }

    private int GetTerrainLayerByName(string name)
    {
        if (nameToTerrainLayer.TryGetValue(name, out int layer))
        {
            return layer;
        }
        else
        {
            return 0;
        }
    }
}
