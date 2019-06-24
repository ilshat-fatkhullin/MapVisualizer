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

    private PriorityQueue<Road> roadsQueue;

    private PriorityQueue<Area> areasQueue;

    private Dictionary<string, int> nameToTerrainLayer;

    private Vector2 terrainSize;

    private List<MultithreadedTerrainPainter> painters;

    private void Start()
    {
        painters = new List<MultithreadedTerrainPainter>();
        InitializeNameToTerrainLayer();
    }

    private void Update()
    {
        for (int i = 0; i < painters.Count; i++)
        {
            if (painters[i].IsCompleted)
            {
                Terrain.terrainData.SetAlphamaps(0, 0, painters[i].Alphamap);
                painters.RemoveAt(i);
                break;
            }
        }
    }

    public void VisualizeTile(Tile tile, List<Road> roads, List<Area> areas, Vector2 originInMeters)
    {
        UpdateTerrainTransform(tile, originInMeters);        
        RetrieveRoads(roads, areas, originInMeters);

        MultithreadedTerrainPainter painter = new MultithreadedTerrainPainter(
            Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight,
            Terrain.terrainData.alphamapLayers, terrainSize, roadsQueue, areasQueue,
            nameToTerrainLayer);

        painters.Add(painter);
        painter.Execute();
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

    private void InitializeNameToTerrainLayer()
    {
        nameToTerrainLayer = new Dictionary<string, int>();

        for (int i = 0; i < Terrain.terrainData.terrainLayers.Length; i++)
        {
            nameToTerrainLayer.Add(Terrain.terrainData.terrainLayers[i].name, i);
        }
    }
}
