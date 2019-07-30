using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VisualizingManager : Singleton<VisualizingManager>
{
    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

    public MultithreadedOsmFileParserEvent OnOsmFileParsed;

    public MultithreadedOsmGeoJSONParserEvent OnGeoJSONParsed;

    public TileEvent OnTileRemoved;

    public Vector2 OriginInMeters { get; private set; }

    public Tilemap Tilemap { get; private set; }

    private List<MultithreadedOsmFileParser> osmFileParsers;

    private List<MultithreadedOsmGeoJSONParser> osmGeoJSONParsers;

    private void Awake()
    {
        OnOsmFileParsed = new MultithreadedOsmFileParserEvent();
        OnGeoJSONParsed = new MultithreadedOsmGeoJSONParserEvent();
        OnTileRemoved = new TileEvent();
        Tilemap = new Tilemap(new Tile(0, 0, Zoom));
    }

    private void Start()
    {
        OriginInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));        

        osmFileParsers = new List<MultithreadedOsmFileParser>();
        osmGeoJSONParsers = new List<MultithreadedOsmGeoJSONParser>();

        NetworkManager.Instance.OnDowloaded.AddListener(OnNetworkResponse);
    }

    private void Update()
    {
        Vector2 cameraPositionInMeters = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        cameraPositionInMeters += OriginInMeters;
        cameraPositionInMeters += new Vector2(-NumericConstants.TILE_SIZE, NumericConstants.TILE_SIZE);

        Tile currentTile = GeoPositioningHelper.GetTileFromCoordinate(
            GeoPositioningHelper.GetCoordinateFromMeters(cameraPositionInMeters),
            Zoom);

        if (Tilemap.CenterTile != currentTile)
        {
            Tilemap.UpdateCenterTile(currentTile);            
        }

        if (Tilemap.HasTilesToRemove())
        {
            OnTileRemoved.Invoke(Tilemap.DequeueTileToRemove());
        }

        if (Tilemap.HasTilesToVisualize())
        {
            DownloadAndVisualizeTile(Tilemap.DequeueTileToVisualize());
        }        

        for (int i = 0; i < osmFileParsers.Count; i++)
        {
            if (osmFileParsers[i].IsCompleted)
            {
                OnOsmFileParsed.Invoke(osmFileParsers[i]);
                osmFileParsers.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < osmGeoJSONParsers.Count; i++)
        {
            if (osmGeoJSONParsers[i].IsCompleted)
            {
                OnGeoJSONParsed.Invoke(osmGeoJSONParsers[i]);
                osmGeoJSONParsers.RemoveAt(i);
                break;
            }
        }
    }

    private void DownloadAndVisualizeTile(Tile tile)
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
}

public class MultithreadedOsmFileParserEvent : UnityEvent<MultithreadedOsmFileParser>
{

}

public class MultithreadedOsmGeoJSONParserEvent : UnityEvent<MultithreadedOsmGeoJSONParser>
{

}

public class TileEvent : UnityEvent<Tile>
{

}
