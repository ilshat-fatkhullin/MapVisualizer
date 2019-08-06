using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Paints all textures based on provided osm file
/// </summary>
public class SurfaceVisualizer : Singleton<TerrainVisualizer>
{
    public GameObject SurfacePrefab;

    [Range(1, 100)]
    public int MapWidth;

    [Range(16, 1024)]
    public int CellTextureResolution;

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
        Texture2D texture = new Texture2D(CellTextureResolution * MapWidth, CellTextureResolution * MapWidth);

        for (int offsetX = 0; offsetX < MapWidth; offsetX += MapWidth)
            for (int offsetY = 0; offsetY < MapWidth; offsetY += MapWidth)
            {
                int layer = surfaceMap[offsetX, offsetY];
                SurfaceLayer surfaceLayer = SurfaceLayers[layer];
                float size = surfaceLayer.Size;

                for (int i = 0; i < CellTextureResolution; i++)
                    for (int j = 0; j < CellTextureResolution; j++)
                    {
                        int x = offsetX + i;
                        int y = offsetY + j;
                        
                        Color color = surfaceLayer.Texture.GetPixelBilinear(x * size, y * size);
                        texture.SetPixel(x, y, color);
                    }
            }

        GameObject tileObject = Instantiate(SurfacePrefab);
        tileObject.transform.localScale = Vector3.one * NumericConstants.TILE_SIZE;
        tileObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
        Vector2 position = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(tile)) - VisualizingManager.Instance.OriginInMeters;
        tileObject.transform.position = new Vector3(position.x, 0, position.y);
        gameObjectTilemap.AttachObjectToTile(tile, tileObject);
    }
}

[Serializable]
public struct SurfaceLayer
{
    public string Name;

    public float Size;

    public Texture2D Texture;
}
