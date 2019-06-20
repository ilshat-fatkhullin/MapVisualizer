using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints all textures based on provided osm file
/// </summary>
public class TerrainVisualizer : Singleton<TerrainVisualizer>
{
    public Terrain Terrain;

    [Range(0, 5)]
    public int DamageBluringDegree;

    public int DamageRarity;

    [Range(0f, 5f)]
    public float DamageStrength;

    private Dictionary<string, Vector2> allNodes;

    private PriorityQueue<Road> roadsQueue;

    private PriorityQueue<Area> areasQueue;

    private Intmap surfaceIntmap;    

    private Vector2 terrainSize;

    private Dictionary<string, int> nameToTerrainLayer;

    private void Awake()
    {
        nameToTerrainLayer = new Dictionary<string, int>();
    }

    public void VisualizeTile(Tile tile, List<Road> roads, List<Area> areas, Vector2 originInMeters)
    {
        UpdateTerrainTransform(tile, originInMeters);
        RetrieveRoads(roads, areas, originInMeters);
        UpdateTerrain();
    }

    private void RetrieveRoads(List<Road> roads, List<Area> areas, Vector2 originInMeters)
    {        
        Vector2 terrainOrigin = new Vector2(Terrain.transform.position.x, Terrain.transform.position.z);

        roadsQueue = new PriorityQueue<Road>();
        areasQueue = new PriorityQueue<Area>();

        foreach (var road in roads)
        {
            roadsQueue.Enqueue(new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(originInMeters + terrainOrigin),
                road.Type));
        }

        foreach (var area in areas)
        {
            areasQueue.Enqueue(new Area(area.GetNodesRelatedToOrigin(originInMeters + terrainOrigin), area.Type));
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
        surfaceIntmap = new Intmap(Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight);

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

            surfaceIntmap.DrawFilledPolygon(points,
                GetTerrainLayerByString(Enum.GetName(typeof(Area.AreaType), area.Type), true));
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
                    GetTerrainLayerByString(Enum.GetName(typeof(Road.RoadType), road.Type), true),
                    MetersToTerrainMapCells(road.GetRoadWidth()));
            }
        }

        float[,,] alphamap = new float[Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight, Terrain.terrainData.alphamapLayers];

        for (int surfaceLayer = 0; surfaceLayer < alphamap.GetLength(2); surfaceLayer++)
        {
            if (!IsSurfaceLayer(surfaceLayer))
                continue;

            for (int x = 0; x < alphamap.GetLength(0); x++)
                for (int y = 0; y < alphamap.GetLength(1); y++)
                {
                    if (surfaceIntmap.Map[x, y] == surfaceLayer)
                    {
                        alphamap[y, x, surfaceLayer] = 1;
                    }
                }
        }

        float[,] damageMap = new float[alphamap.GetLength(0), alphamap.GetLength(1)];

        for (int x = 0; x < damageMap.GetLength(0); x++)
            for (int y = 0; y < damageMap.GetLength(1); y++)
            {
                if (UnityEngine.Random.Range(0, DamageRarity) != 0)
                    continue;

                 damageMap[x, y] = UnityEngine.Random.Range(0f, DamageStrength);    
            }

        for (int i = 0; i < DamageBluringDegree; i++)
        {
            BlurHelper.Blur(damageMap);
        }        

        for (int x = 0; x < damageMap.GetLength(0); x++)
            for (int y = 0; y < damageMap.GetLength(1); y++)
            {
                int damageLayer = GetDamageLayer(surfaceIntmap.Map[x, y]);
                if (damageLayer != 0)
                {
                    alphamap[y, x, damageLayer] = Mathf.Clamp(damageMap[x, y], 0, 1);
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

    private int GetTerrainLayerByString(string name, bool isSurface)
    {
        string n;

        if (isSurface)
        {
            n = "S_" + name;
        }
        else
        {
            n = "D_" + name;
        }

        if (nameToTerrainLayer.ContainsKey(n))
        {
            return nameToTerrainLayer[n];
        }

        int layer = 0;

        for (int i = 0; i < Terrain.terrainData.terrainLayers.Length; i++)
        {
            if (Terrain.terrainData.terrainLayers[i].name == n)
            {
                layer = i;
                break;
            }
        }

        nameToTerrainLayer.Add(n, layer);
        return layer;
    }

    private bool IsSurfaceLayer(int layer)
    {
        return Terrain.terrainData.terrainLayers[layer].name[0] == 'S';
    }

    private int GetDamageLayer(int layer)
    {
        return GetTerrainLayerByString(Terrain.terrainData.terrainLayers[layer].name.Substring(2), false);
    }
}
