using System;
using System.Collections.Generic;
using UnityEngine;

public class MultithreadedSurfacePainter : MultithreadedEntity
{
    public int[,] SurfaceMap;

    public Tile Tile;

    private int mapWidth;

    private PriorityQueue<Road> roadsQueue;

    private PriorityQueue<Area> areasQueue;

    private Dictionary<string, int> nameToTerrainLayer;

    public MultithreadedSurfacePainter(int mapWidth,
        Tile tile, PriorityQueue<Road> roadsQueue, PriorityQueue<Area> areasQueue,
        Dictionary<string, int> nameToTerrainLayer)
    {
        Tile = tile;
        this.mapWidth = mapWidth;
        this.roadsQueue = roadsQueue;
        this.areasQueue = areasQueue;
        this.nameToTerrainLayer = nameToTerrainLayer;
    }

    protected override void ExecuteThread()
    {
        Intmap surfaceIntmap = new Intmap(mapWidth, mapWidth);

        while (areasQueue.Count() > 0)
        {
            Area area = areasQueue.Dequeue();

            if (area.Nodes.Length < 3)
                continue;

            Point2D[] points = new Point2D[area.Nodes.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetSurfaceMapCell(area.Nodes[i]);
            }

            surfaceIntmap.DrawFilledPolygon(points, GetSurfaceLayerByName(Enum.GetName(typeof(Area.AreaType), area.Type)));
        }

        while (roadsQueue.Count() > 0)
        {
            Road road = roadsQueue.Dequeue();

            if (road.Nodes.Length < 2)
                continue;

            for (int i = 1; i < road.Nodes.Length; i++)
            {
                Point2D point1 = GetSurfaceMapCell(road.Nodes[i - 1]);
                Point2D point2 = GetSurfaceMapCell(road.Nodes[i]);

                surfaceIntmap.DrawLine(point1, point2,
                                       GetSurfaceLayerByName(Enum.GetName(typeof(Road.RoadType), road.Type)),
                                       MetersToSurfaceMapCells(road.GetRoadWidth()));
            }
        }

        SurfaceMap = surfaceIntmap.Map;
    }

    private Point2D GetSurfaceMapCell(Vector2 coordinate)
    {
        return new Point2D(Mathf.RoundToInt((coordinate.x / NumericConstants.TILE_SIZE) * mapWidth),
                           Mathf.RoundToInt((coordinate.y / NumericConstants.TILE_SIZE) * mapWidth));
    }

    private int MetersToSurfaceMapCells(float meters)
    {
        return Mathf.RoundToInt(meters * mapWidth / NumericConstants.TILE_SIZE);
    }

    private int GetSurfaceLayerByName(string name)
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
