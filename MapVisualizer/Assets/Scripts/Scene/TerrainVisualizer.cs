using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints all textures based on provided osm file
/// </summary>
public class TerrainVisualizer : Singleton<TerrainVisualizer>
{
    public Terrain Terrain;    

    private Dictionary<string, Vector2> allNodes;

    private PriorityQueue<Road> roads;

    private PriorityQueue<Area> areas;

    private Intmap intmap;

    private Vector2 terrainSize;

    private Dictionary<string, int> nameToTerrainLayer;

    private void Awake()
    {
        nameToTerrainLayer = new Dictionary<string, int>();
    }

    public void VisualizeTile(Tile tile, OsmFile file, Vector2 originInMeters)
    {
        UpdateTerrainTransform(tile, originInMeters);
        RetrieveRoads(file, originInMeters);
        UpdateTerrain();
    }

    private void RetrieveRoads(OsmFile file, Vector2 originInMeters)
    {        
        Vector2 terrainOrigin = new Vector2(Terrain.transform.position.x, Terrain.transform.position.z);

        roads = new PriorityQueue<Road>();
        areas = new PriorityQueue<Area>();

        foreach (OsmWay way in file.GetWays())
        {
            List<OsmNode> nodes = way.GetNodes();
            Vector2[] points = new Vector2[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                points[i] = nodes[i].Coordinate - originInMeters - terrainOrigin;
            }

            int lanes = 1;
            Road.RoadType roadType = Road.RoadType.Default;
            Area.AreaType areaType = Area.AreaType.Default;

            string lanesStr = way.GetTagValue("lanes");            
            string highwaysStr = way.GetTagValue("highway");
            string landuseStr = way.GetTagValue("landuse");
            string leisureStr = way.GetTagValue("leisure");

            if (lanesStr != null)
            {
                lanes = Convert.ToInt32(lanesStr);
            }            

            if (highwaysStr != null)
            {
                roadType = Road.GetRoadType(highwaysStr);
            }

            if (landuseStr != null)
            {
                areaType = Area.GetAreaType(landuseStr);
            }

            if (leisureStr != null)
            {
                areaType = Area.GetAreaType(leisureStr);
            }

            if (areaType != Area.AreaType.Default)
            {
                areas.Enqueue(new Area(points, areaType));
            }

            if (roadType != Road.RoadType.Default)
            {
                roads.Enqueue(new Road(lanes, points, roadType));
            }
        }
    }

    private void UpdateTerrainTransform(Tile tile, Vector2 originInMeters)
    {
        Terrain.Flush();

        Vector2 nw = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(tile));
        Vector2 se = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(tile.X + 1, tile.Y + 1, tile.Zoom)));
        terrainSize = new Vector2(Math.Abs(nw.x - se.x), Math.Abs(nw.y - se.y));

        Terrain.terrainData.size = new Vector3(terrainSize.x, Terrain.terrainData.size.y, terrainSize.y);

        Terrain.gameObject.transform.position = new Vector3(nw.x - originInMeters.x, 0, se.y - originInMeters.y);
    }

    private void UpdateTerrain()
    {        
        intmap = new Intmap(Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight);

        while (areas.Count() > 0)
        {
            Area area = areas.Dequeue();

            if (area.Nodes.Length < 3)
                continue;

            Point2D[] points = new Point2D[area.Nodes.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetTerrainMapPoint(area.Nodes[i]);
            }

            intmap.DrawFilledPolygon(points, GetTerrainLayerByString(Enum.GetName(typeof(Area.AreaType), area.Type)));
        }

        while (roads.Count() > 0)
        {
            Road road = roads.Dequeue();

            if (road.Nodes.Length < 2)
                continue;

            for (int i = 1; i < road.Nodes.Length; i++)
            {
                Point2D point1 = GetTerrainMapPoint(road.Nodes[i - 1]);
                Point2D point2 = GetTerrainMapPoint(road.Nodes[i]);

                intmap.DrawLine(point1, point2,
                                GetTerrainLayerByString(Enum.GetName(typeof(Road.RoadType), road.Type)),
                                MetersToTerrainMapCells(road.GetRoadWidth()));
            }
        }

        float[,,] alphamap = new float[Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight, Terrain.terrainData.alphamapLayers];

        for (int x = 0; x < alphamap.GetLength(0); x++)
            for (int y = 0; y < alphamap.GetLength(1); y++)
                for (int l = 0; l < alphamap.GetLength(2); l++)
                {
                    if (intmap.Map[x, y] == l)
                    {
                        alphamap[y, x, l] = 1;
                    }
                    else
                    {
                        alphamap[y, x, l] = 0;
                    }
                }

        Terrain.terrainData.SetAlphamaps(0, 0, alphamap);
    }

    private Point2D GetTerrainMapPoint(Vector2 coordinate)
    {
        return new Point2D(Mathf.RoundToInt((coordinate.x / terrainSize.x) * Terrain.terrainData.alphamapWidth),
                           Mathf.RoundToInt((coordinate.y / terrainSize.y) * Terrain.terrainData.alphamapHeight));
    }

    private int MetersToTerrainMapCells(float meters)
    {
        return Mathf.RoundToInt(meters * Terrain.terrainData.alphamapWidth / terrainSize.x);
    }

    private int GetTerrainLayerByString(string name)
    {
        if (nameToTerrainLayer.ContainsKey(name))
        {
            return nameToTerrainLayer[name];
        }

        int layer = 0;

        for (int i = 0; i < Terrain.terrainData.terrainLayers.Length; i++)
        {
            if (Terrain.terrainData.terrainLayers[i].name == name)
            {
                layer = i;
                break;
            }
        }

        nameToTerrainLayer.Add(name, layer);
        return layer;
    }
}
