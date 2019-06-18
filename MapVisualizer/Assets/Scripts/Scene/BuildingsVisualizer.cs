using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static BuildingPropertiesHelper;

public class BuildingsVisualizer : Singleton<BuildingsVisualizer>
{
    public GameObject BuildingPrefab;

    public Material[] WallMaterials;

    public Material RoofMaterial;

    private List<GameObject> buildings;

    private void Awake()
    {
        buildings = new List<GameObject>();
    }

    public void VisualizeTile(Tile tile, FeatureCollection featureCollection, Vector2 originInMeters)
    {
        foreach (var building in buildings)
        {
            Destroy(building);
        }        

        foreach (var feature in featureCollection.Features)
        {
            InstantiateGeometry(feature.Geometry, feature.Properties, tile, originInMeters);
        }
    }

    private void InstantiateGeometry(Geometry geometry, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
    {
        switch (geometry.Type)
        {
            case GeoJsonType.Polygon:
                InstantiatePolygon(geometry as Polygon, properties, tile, originInMeters);
                break;
            case GeoJsonType.GeometryCollection:
                InstantiateGeometryCollection(geometry as GeometryCollection, properties, tile, originInMeters);
                break;
            default:
                Debug.LogError("Invalid type of geometry to be instantiated");
                break;
        }
    }

    private void InstantiatePolygon(Polygon polygon, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
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

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile, originInMeters);
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
