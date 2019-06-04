using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static BuildingPropertiesHelper;

public class GeoJSONBuildingsVisualizer : Visualizer
{
    public Material[] WallMaterials;

    public Material RoofMaterial;

    private FeatureCollection tileGeoJSON;

    private Tile currentTile;

    protected override void Start()
    {
        base.Start();
        currentTile = new Tile(0, 0, 0);
    }

    private void VisualizeTile()
    {
        string request = currentTile.BuildRequest();
        Debug.Log(request);
        StartCoroutine(LoadFile(request));
    }

    private void Update()
    {
        Vector2 cameraPositionInMeters = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        cameraPositionInMeters += originInMeters;
        Tile tile = GeoPositioningHelper.GetTileFromCoordinate(
            GeoPositioningHelper.GetCoordinateFromMeters(cameraPositionInMeters),
            Zoom);
        if (currentTile.X != tile.X || currentTile.Y != tile.Y)
        {
            currentTile = tile;
            VisualizeTile();
        }
    }

    protected override void OnNetworkResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Invalid response received");
        }

        tileGeoJSON = JsonConvert.DeserializeObject<FeatureCollection>(response);

        foreach (var feature in tileGeoJSON.Features)
        {
            InstantiateGeometry(feature.Geometry, feature.Properties);
        }
    }

    private void InstantiateGeometry(Geometry geometry, IDictionary<string, dynamic> properties)
    {
        switch (geometry.Type)
        {
            case GeoJsonType.Polygon:
                InstantiatePolygon(geometry as Polygon, properties);
                break;
            case GeoJsonType.GeometryCollection:
                InstantiateGeometryCollection(geometry as GeometryCollection, properties);
                break;
            default:
                Debug.LogError("Invalid type of geometry to be instantiated");
                break;
        }
    }

    private void InstantiatePolygon(Polygon polygon, IDictionary<string, dynamic> properties)
    {
        PolygonLoops polygonLoops = GetPolygonLoopsInMeters(polygon, originInMeters);

        MeshInfo roofInfo = GetRoofInfo(polygonLoops, properties);
        MeshInfo wallInfo = GetWallInfo(polygonLoops, properties);

        InstantiateObject(roofInfo, RoofMaterial);
        InstantiateObject(wallInfo, WallMaterials[UnityEngine.Random.Range(0, WallMaterials.Length)]);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties);
        }
    }
}
