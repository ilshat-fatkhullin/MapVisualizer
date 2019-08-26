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

    private Vector2 tileSizeInMeters;

    private List<MultithreadedTerrainPainter> painters;

    private Dictionary<Tile, float[,,]> calculatedTiles;

    private int tileAlphamapWidth;

    private void Start()
    {
        calculatedTiles = new Dictionary<Tile, float[,,]>(new TileEqualityComparer());
        painters = new List<MultithreadedTerrainPainter>();

        InitializeNameToTerrainLayer();

        VisualizingManager.Instance.OnOsmFileParsed.AddListener(VisualizeTile);
        VisualizingManager.Instance.OnTileRemoved.AddListener(RemoveTile);
        VisualizingManager.Instance.Tilemap.OnCenterTileChanged.AddListener(OnCenterTileChanged);

        tileAlphamapWidth = Terrain.terrainData.alphamapWidth / 3;
    }

    private void Update()
    {
        for (int i = 0; i < painters.Count; i++)
        {
            if (painters[i].IsCompleted)
            {
                if (calculatedTiles.ContainsKey(painters[i].Tile))
                    continue;
                calculatedTiles.Add(painters[i].Tile, painters[i].Alphamap);
                SetTileAlphamap(painters[i].Tile, painters[i].Alphamap);
                painters.RemoveAt(i);
                break;
            }
        }
    }

    private void SetTileAlphamap(Tile tile, float[,,] alphamap)
    {
        Tile centerTile = VisualizingManager.Instance.Tilemap.CenterTile;
        Tile t = new Tile(tile.X - centerTile.X + 1, tile.Y - centerTile.Y + 1, tile.Zoom);
        Terrain.terrainData.SetAlphamaps(t.X * tileAlphamapWidth, (2 - t.Y) * tileAlphamapWidth, alphamap);
    }

    private void OnCenterTileChanged()
    {
        Tile centerTile = VisualizingManager.Instance.Tilemap.CenterTile;
        for (int x = centerTile.X - 1; x <= centerTile.X + 1; x++)
            for (int y = centerTile.Y - 1; y <= centerTile.Y + 1; y++)
            {
                Tile tile = new Tile(x, y, centerTile.Zoom);
                if (calculatedTiles.ContainsKey(tile))
                {
                    SetTileAlphamap(tile, calculatedTiles[tile]);
                }
            }
    }

    public void VisualizeTile(MultithreadedOsmFileParser parser)
    {
        UpdateTerrainTransform(VisualizingManager.Instance.Tilemap.CenterTile);
        RetrieveRoads(parser.Tile, parser.Roads, parser.Areas);

        MultithreadedTerrainPainter painter = new MultithreadedTerrainPainter(
            tileAlphamapWidth, Terrain.terrainData.alphamapLayers, parser.Tile,
            tileSizeInMeters, roadsQueue, areasQueue, nameToTerrainLayer);

        painters.Add(painter);
        painter.Execute();
    }

    public void RemoveTile(Tile tile)
    {
        for (int i = 0; i < painters.Count; i++)
        {
            if (painters[i].Tile == tile)
            {
                painters[i].Stop();
                painters.RemoveAt(i);
                i--;
            }
        }

        if (calculatedTiles.ContainsKey(tile))            
            calculatedTiles.Remove(tile);
    }

    private void RetrieveRoads(Tile tile, List<Road> roads, List<Area> areas)
    {        
        Vector2 terrainOrigin = new Vector2(Terrain.transform.position.x,
                                            Terrain.transform.position.z);

        Tile centerTile = VisualizingManager.Instance.Tilemap.CenterTile;

        terrainOrigin += new Vector2(tile.X - centerTile.X + 1, 1 - tile.Y + centerTile.Y) * Terrain.terrainData.size.x / 3f;

        roadsQueue = new PriorityQueue<Road>();
        areasQueue = new PriorityQueue<Area>();

        foreach (var road in roads)
        {
            roadsQueue.Enqueue(new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(terrainOrigin + VisualizingManager.Instance.OriginInMeters),
                road.Type));
        }

        foreach (var area in areas)
        {
            areasQueue.Enqueue(new Area(
                area.GetNodesRelatedToOrigin(terrainOrigin + VisualizingManager.Instance.OriginInMeters), area.Type));
        }
    }

    private void UpdateTerrainTransform(Tile centerTile)
    {
        Vector2 nw = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(centerTile));
        Vector2 se = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(centerTile.X + 1, centerTile.Y + 1, centerTile.Zoom)));

        tileSizeInMeters = new Vector2(Math.Abs(nw.x - se.x), Math.Abs(nw.y - se.y));

        Vector2 position = (nw + se) / 2 - VisualizingManager.Instance.OriginInMeters;

        Terrain.terrainData.size = new Vector3(tileSizeInMeters.x * 3, Terrain.terrainData.size.y, tileSizeInMeters.y * 3);
        Terrain.gameObject.transform.position = new Vector3(position.x - Terrain.terrainData.size.x / 2, 0,
                                                            position.y - Terrain.terrainData.size.z / 2);
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
