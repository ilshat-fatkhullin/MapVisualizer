using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints all textures based on provided osm file
/// </summary>
public class SurfaceVisualizer : MonoBehaviour
{
    public static SurfaceVisualizer Instance { get { return Singleton<SurfaceVisualizer>.Instance; } }

    /// <summary>
    /// Amount of cells per tile.
    /// </summary>
    [Range(1, 100)]
    public int MapWidth;

    public GameObject SurfacePrefab;

    /// <summary>
    /// Information about correspondance between layer and its material.
    /// </summary>
    public SurfaceLayer[] SurfaceLayers;

    private Dictionary<string, Vector2> allNodes;

    private PriorityQueue<Road> roadsQueue;

    private PriorityQueue<Area> areasQueue;

    private GameObjectTilemap gameObjectTilemap;

    private Dictionary<string, int> nameToSurfaceLayer;

    private List<MultithreadedSurfacePainter> painters;

    private void Start()
    {
        gameObjectTilemap = new GameObjectTilemap();
        InitializeNameToSurfaceLayer();
        painters = new List<MultithreadedSurfacePainter>();

        VisualizingManager.Instance.OnOsmFileParsed.AddListener(VisualizeTile);
        VisualizingManager.Instance.OnTileRemoved.AddListener(gameObjectTilemap.RemoveTile);
    }

    private void Update()
    {
        for (int i = 0; i < painters.Count; i++)
        {
            if (painters[i].IsCompleted)
            {
                InstantiateTile(painters[i].Tile, painters[i].SurfaceMap);
                painters.RemoveAt(i);
                return;
            }
        }
    }

    private void VisualizeTile(MultithreadedOsmFileParser parser)
    {
        RetrieveRoads(parser.Tile, parser.Roads, parser.Areas);
        MultithreadedSurfacePainter painter = new MultithreadedSurfacePainter(
            MapWidth, parser.Tile, roadsQueue, areasQueue, nameToSurfaceLayer);
        painters.Add(painter);
        painter.Execute();
    }

    private void RetrieveRoads(Tile tile, List<Road> roads, List<Area> areas)
    {
        Tile centerTile = VisualizingManager.Instance.Tilemap.CenterTile;
        Vector2 tileOrigin = new Vector2(tile.X - centerTile.X, tile.Y - centerTile.Y) * NumericConstants.TILE_SIZE;

        roadsQueue = new PriorityQueue<Road>();
        areasQueue = new PriorityQueue<Area>();

        foreach (var road in roads)
        {
            roadsQueue.Enqueue(new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(tileOrigin + VisualizingManager.Instance.OriginInMeters),
                road.Type));
        }

        foreach (var area in areas)
        {
            areasQueue.Enqueue(new Area(
                area.GetNodesRelatedToOrigin(tileOrigin + VisualizingManager.Instance.OriginInMeters), area.Type));
        }
    }

    private void InitializeNameToSurfaceLayer()
    {
        nameToSurfaceLayer = new Dictionary<string, int>();

        for (int i = 0; i < SurfaceLayers.Length; i++)
        {
            nameToSurfaceLayer.Add(SurfaceLayers[i].Name, i);
        }
    }

    private void InstantiateTile(Tile tile, int[,] surfaceMap)
    {
        SurfaceBuilder[] surfaceBuilders = new SurfaceBuilder[SurfaceLayers.Length];

        for (int i = 0; i < surfaceBuilders.Length; i++)
        {
            SurfaceLayer surfaceLayer = SurfaceLayers[i];
            surfaceBuilders[i] = new SurfaceBuilder(surfaceLayer.Material, surfaceLayer.Tiling);
        }

        for (int offsetX = 0; offsetX < MapWidth; offsetX++)
            for (int offsetY = 0; offsetY < MapWidth; offsetY++)
            {
                int layer = surfaceMap[offsetX, offsetY];
                surfaceBuilders[layer].AddCell(new Point2D(offsetX, offsetY));
            }
        
        Vector2 position2D = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(tile)) - VisualizingManager.Instance.OriginInMeters;
        Vector3 position = new Vector3(position2D.x, 0, position2D.y);
        Vector2 tileSize = GeoPositioningHelper.GetTileSizeInMeters(tile);
        Vector3 objectSize = new Vector3(tileSize.x / MapWidth, 0, tileSize.y / MapWidth);

        foreach (var builder in surfaceBuilders)
        {
            if (builder.IsEmpty())
                continue;

            GameObject tileObject = builder.Instantiate(position);            
            tileObject.transform.localScale = objectSize;
            gameObjectTilemap.AttachObjectToTile(tile, tileObject);
        }
    }
}

[Serializable]
public struct SurfaceLayer
{
    public string Name;

    public float Tiling;

    public Material Material;
}
