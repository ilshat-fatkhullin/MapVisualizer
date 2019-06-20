using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class VisualizingManager : Singleton<VisualizingManager>
{
    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

    protected Vector2 originInMeters;

    protected Tile currentTile;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
        currentTile = new Tile(0, 0, 0);
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
            VisualizeTile(currentTile);
        }
    }

    private void VisualizeTile(Tile tile)
    {
        NetworkManager.Instance.DownloadTile(tile, NetworkManager.RequestType.GeoJSON);
        NetworkManager.Instance.DownloadTile(tile, NetworkManager.RequestType.OsmFile);
        NetworkManager.Instance.OnDowloaded.AddListener(OnNetworkResponse);
    }

    private void OnNetworkResponse(Tile tile, NetworkManager.RequestType type, string response)
    {
        switch (type)
        {
            case NetworkManager.RequestType.OsmFile:
                OsmFile osmFile = new OsmFile(response);

                List<Road> roads = OsmFileParser.GetRoads(osmFile);
                List<Area> areas = OsmFileParser.GetAreas(osmFile);

                TerrainVisualizer.Instance.VisualizeTile(tile, roads, areas, originInMeters);
                BarriersVisualizer.Instance.VisualizeTile(tile, osmFile, originInMeters);
                MarkupVisualizer.Instance.VisualizeTile(tile, roads, originInMeters);
                break;
            case NetworkManager.RequestType.GeoJSON:
                FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(response);
                BuildingsVisualizer.Instance.VisualizeTile(tile, featureCollection, originInMeters);
                break;
        }        
    }
}
