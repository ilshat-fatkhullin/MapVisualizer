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
    [Range(1, 200)]
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
        Vector2 tileOrigin = GeoPositioningHelper.GetMetersFromCoordinate(
            GeoPositioningHelper.GetNWCoordinateFromTile(
                new Tile(tile.X, tile.Y + 1, tile.Zoom)));

        roadsQueue = new PriorityQueue<Road>();
        areasQueue = new PriorityQueue<Area>();

        foreach (var road in roads)
        {
            roadsQueue.Enqueue(new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(tileOrigin),
                road.Type));
        }

        foreach (var area in areas)
        {
            areasQueue.Enqueue(new Area(
                area.GetNodesRelatedToOrigin(tileOrigin), area.Type));
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
        Vector2 tileSize = GeoPositioningHelper.GetTileSizeInMeters(tile);
        Vector2 position2D = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(tile)) - VisualizingManager.Instance.OriginInMeters;
        Vector3 position = new Vector3(position2D.x + tileSize.x / 2, 0, position2D.y - tileSize.y / 2);
        Vector3 scale = new Vector3(tileSize.x, 0, tileSize.y) / 10;

        for (int l = 0; l < SurfaceLayers.Length; l++)
        {

            Texture2D mask = new Texture2D(surfaceMap.GetLength(0), surfaceMap.GetLength(1));

            for (int i = 0; i < surfaceMap.GetLength(0); i++)
                for (int j = 0; j < surfaceMap.GetLength(1); j++)
                {
                    if (surfaceMap[surfaceMap.GetLength(0) - 1 - i, surfaceMap.GetLength(1) - 1 - j] == l)
                    {
                        mask.SetPixel(i, j, Color.white);
                    }
                    else
                    {
                        mask.SetPixel(i, j, Color.clear);
                    }
                }

            mask.Apply();
        
            GameObject tileObject = Instantiate(SurfacePrefab);
            Material material = tileObject.GetComponent<MeshRenderer>().material;
            material.mainTextureScale = Vector2.one * SurfaceLayers[l].Tiling;
            material.SetTexture("_MainTex", SurfaceLayers[l].Texture);
            material.SetTexture("_Alpha", mask);
            tileObject.transform.position = position;
            tileObject.transform.localScale = scale;
            gameObjectTilemap.AttachObjectToTile(tile, tileObject);
        }
    }
}

[Serializable]
public struct SurfaceLayer
{
    public string Name;

    public float Tiling;

    public Texture Texture;
}
