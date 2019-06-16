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

    private List<GameObject> buildings;

    private void Awake()
    {
        buildings = new List<GameObject>();
    }

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

        foreach (var building in buildings)
        {
            Destroy(building);
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

        InstantiateBuilding(roofInfo, RoofMaterial);
        InstantiateBuilding(wallInfo, WallMaterials[UnityEngine.Random.Range(0, WallMaterials.Length)]);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile);
        }
    }

    private void InstantiateBuilding(MeshInfo info, Material material)
    {
        GameObject building = Instantiate(BuildingPrefab);
        MeshFilter filter = building.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = info.Vertices;
        mesh.triangles = info.Triangles;
        mesh.SetUVs(0, new List<Vector2>(info.UVs));
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        filter.mesh = mesh;
        MeshRenderer renderer = building.GetComponent<MeshRenderer>();
        renderer.material = material;
        buildings.Add(building);
    }
}
