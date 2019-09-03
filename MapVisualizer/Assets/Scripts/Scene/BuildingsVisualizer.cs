using BAMCIS.GeoJSON;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsVisualizer : Singleton<BuildingsVisualizer>
{
    public GameObject RoofPrefab;

    public GameObject WallPrefab;

    public Material WallMaterial;

    public Material RoofMaterial;

    private GameObjectTilemap gameObjectTilemap; 

    private void Start()
    {
        gameObjectTilemap = new GameObjectTilemap();
        VisualizingManager.Instance.OnGeoJSONParsed.AddListener(VisualizeTile);
        VisualizingManager.Instance.OnTileRemoved.AddListener(gameObjectTilemap.RemoveTile);
    }

    public void VisualizeTile(MultithreadedOsmGeoJSONParser parser)
    {
        if (parser.FeatureCollection == null)
            return;

        foreach (var feature in parser.FeatureCollection.Features)
        {
            InstantiateGeometry(feature.Geometry, feature.Properties, parser.Tile,
                                VisualizingManager.Instance.OriginInMeters);
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
        PolygonLoops polygonLoops = BuildingPropertiesHelper.GetPolygonLoopsInMeters(polygon, originInMeters);

        int levels = BuildingPropertiesHelper.GetNumberOfLevels(properties);
        float height = levels * NumericConstants.LEVEL_HEIGHT;

        MeshInfo roofInfo;
        MeshInfo wallInfo;

        try
        {            
            roofInfo = BuildingPropertiesHelper.GetRoofInfo(polygonLoops, height);
            wallInfo = BuildingPropertiesHelper.GetWallInfo(polygonLoops, height);
        }
        catch
        {
            Debug.Log("Some roofs are not visualized.");
            return;
        }

        InstantiateObject(tile, roofInfo, RoofMaterial, RoofPrefab);
        InstantiateObject(tile, wallInfo, WallMaterial, WallPrefab);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile, originInMeters);
        }
    }

    private GameObject InstantiateObject(Tile tile, MeshInfo info, Material material, GameObject prefab)
    {
        Vector3 position = Vector3.zero;

        foreach (var v in info.Vertices)
        {
            position += v;
        }

        position /= info.Vertices.Length;

        for (int i = 0; i < info.Vertices.Length; i++)
        {
            info.Vertices[i] -= position;
        }

        GameObject building = Instantiate(prefab, position, Quaternion.identity);        
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
        gameObjectTilemap.AttachObjectToTile(tile, building);
        return building;
    }
}
