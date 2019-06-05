using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BuildingPropertiesHelper;

public class GeoJSONBuildingsVisualizer : Visualizer
{
    public Material[] WallMaterials;

    public Material RoofMaterial;

    protected override string BuildRequest(Tile tile)
    {
        return string.Format(StringConstants.GeoJSONTileRequestPattern,
            Convert.ToString(tile.Zoom), Convert.ToString(tile.X), Convert.ToString(tile.Y));
    }

    protected override void OnNetworkResponse(Tile tile, string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return;
        }

        FeatureCollection tileGeoJSON = JsonConvert.DeserializeObject<FeatureCollection>(response);

        foreach (var feature in tileGeoJSON.Features)
        {
            InstantiateGeometry(feature.Geometry, feature.Properties, tile);
        }
    }

    private void InstantiateGeometry(Geometry geometry, IDictionary<string, dynamic> properties, Tile tile)
    {
        switch (geometry.Type)
        {
            case GeoJsonType.Polygon:
                InstantiatePolygon(geometry as Polygon, properties, tile);
                break;
            case GeoJsonType.GeometryCollection:
                InstantiateGeometryCollection(geometry as GeometryCollection, properties, tile);
                break;
            default:
                Debug.LogError("Invalid type of geometry to be instantiated");
                break;
        }
    }

    private void InstantiatePolygon(Polygon polygon, IDictionary<string, dynamic> properties, Tile tile)
    {
        PolygonLoops polygonLoops = GetPolygonLoopsInMeters(polygon, originInMeters);
        MeshInfo roofInfo, wallInfo;

        try
        {
            roofInfo = GetRoofInfo(polygonLoops, properties);
            wallInfo = GetWallInfo(polygonLoops, properties);
        }
        catch
        {
            Debug.Log("Some buildings are not visualized.");
            return;
        }

        InstantiateObject(roofInfo, RoofMaterial, tile);
        InstantiateObject(wallInfo, WallMaterials[UnityEngine.Random.Range(0, WallMaterials.Length)], tile);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile);
        }
    }
}
