using System.Collections.Generic;
using UnityEngine;

public class VisualizingManager : Singleton<VisualizingManager>
{
    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

    private Vector2 originInMeters;

    private Tile currentTile;

    private List<MultithreadedOsmFileParser> osmFileParsers;

    private List<MultithreadedOsmGeoJSONParser> osmGeoJSONParsers;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
        currentTile = new Tile(0, 0, 0);

        osmFileParsers = new List<MultithreadedOsmFileParser>();
        osmGeoJSONParsers = new List<MultithreadedOsmGeoJSONParser>();

        NetworkManager.Instance.OnDowloaded.AddListener(OnNetworkResponse);
    }

    private void Update()
    {
        Vector2 cameraPositionInMeters = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        cameraPositionInMeters += originInMeters;
        cameraPositionInMeters += new Vector2(-NumericConstants.TILE_SIZE, NumericConstants.TILE_SIZE);

        Tile currentTile = GeoPositioningHelper.GetTileFromCoordinate(
            GeoPositioningHelper.GetCoordinateFromMeters(cameraPositionInMeters),
            Zoom);

        if (this.currentTile != currentTile)
        {
            this.currentTile = currentTile;
            DownloadTile(currentTile);
        }

        for (int i = 0; i < osmFileParsers.Count; i++)
        {
            if (osmFileParsers[i].IsCompleted)
            {
                VisualizeOsmFile(osmFileParsers[i]);
                osmFileParsers.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < osmGeoJSONParsers.Count; i++)
        {
            if (osmGeoJSONParsers[i].IsCompleted)
            {
                VisualizeGeoJSON(osmGeoJSONParsers[i]);
                osmGeoJSONParsers.RemoveAt(i);
                break;
            }
        }
    }

    private void DownloadTile(Tile tile)
    {
        NetworkManager.Instance.DownloadTile(tile, NetworkManager.RequestType.GeoJSON);
        NetworkManager.Instance.DownloadTile(tile, NetworkManager.RequestType.OsmFile);        
    }

    private void OnNetworkResponse(Tile tile, NetworkManager.RequestType type, string response)
    {
        switch (type)
        {
            case NetworkManager.RequestType.OsmFile:
                MultithreadedOsmFileParser osmFileParser = new MultithreadedOsmFileParser(tile, response);
                osmFileParsers.Add(osmFileParser);
                osmFileParser.Execute();
                break;
            case NetworkManager.RequestType.GeoJSON:
                MultithreadedOsmGeoJSONParser osmGeoJSONParser = new MultithreadedOsmGeoJSONParser(tile, response);
                osmGeoJSONParsers.Add(osmGeoJSONParser);
                osmGeoJSONParser.Execute();
                break;
        }
        
    }

    private void VisualizeOsmFile(MultithreadedOsmFileParser osmFileParser)
    {
        TerrainVisualizer.Instance.VisualizeTile(osmFileParser.Tile, osmFileParser.Roads, osmFileParser.Areas, originInMeters);
        BarriersVisualizer.Instance.VisualizeTile(osmFileParser.Tile, osmFileParser.OsmFile, originInMeters);
        MarkupVisualizer.Instance.VisualizeTile(osmFileParser.Tile, osmFileParser.Roads, originInMeters);
    }

    private void VisualizeGeoJSON(MultithreadedOsmGeoJSONParser osmGeoJSONParser)
    {
        BuildingsVisualizer.Instance.VisualizeTile(osmGeoJSONParser.Tile, osmGeoJSONParser.FeatureCollection, originInMeters);
    }
}
