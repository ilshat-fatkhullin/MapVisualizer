using BAMCIS.GeoJSON;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsVisualizer : Singleton<BuildingsVisualizer>
{
    public GameObject RoofPrefab;

    public GameObject WallPrefab;

    public Material WallMaterial;

    public Material RoofMaterial;

    public GameObject FacadeContainerPrefab;

    public GameObject[] BaseFacadePrefabs;

    public GameObject[] MiddleFacadePrefabs;

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
        float height;

        GameObject baseFacadePrefab = BaseFacadePrefabs[Random.Range(0, BaseFacadePrefabs.Length)];
        GameObject middleFacadePrefab = MiddleFacadePrefabs[Random.Range(0, MiddleFacadePrefabs.Length)];

        if (levels <= 1)
        {
            height = middleFacadePrefab.transform.localScale.y;
        }
        else
        {
            height = baseFacadePrefab.transform.localScale.y + middleFacadePrefab.transform.localScale.y * (levels - 1);
        }

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
        InstantiateWalls(tile, wallInfo, polygonLoops.OuterLoop, levels, baseFacadePrefab, middleFacadePrefab);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile, originInMeters);
        }
    }

    private void InstantiateWalls(Tile tile, MeshInfo wallInfo, Vector2[] points, int levels,
                                  GameObject baseFacade, GameObject middleFacade)
    {
        GameObject wall = InstantiateObject(tile, wallInfo, WallMaterial, WallPrefab);

        float y;

        GameObject facadeContainer = Instantiate(FacadeContainerPrefab);
        facadeContainer.transform.parent = wall.transform;

        if (levels == 1)
        {
            InstantiateFacade(tile, points, middleFacade.transform.lossyScale.y / 2,
                                middleFacade, facadeContainer.transform);
            return;
        }
        else
        {
            InstantiateFacade(tile, points, baseFacade.transform.lossyScale.y / 2,
                                baseFacade, facadeContainer.transform);

            facadeContainer = Instantiate(FacadeContainerPrefab);
            facadeContainer.transform.parent = wall.transform;

            y = baseFacade.transform.lossyScale.y;
        }

        for (int i = 1; i < levels; i++)
        {
            InstantiateFacade(tile, points, y + middleFacade.transform.lossyScale.y / 2,
                                middleFacade, facadeContainer.transform);

            y += middleFacade.transform.lossyScale.y;
        }
    }

    private void InstantiateFacade(Tile tile, Vector2[] points, float y, GameObject facadePrefab, Transform parent)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 pointA2D = points[i % points.Length];
            Vector2 pointB2D = points[(i + 1) % points.Length];
            Vector2 direction2D = pointB2D - pointA2D;
            float length = direction2D.magnitude;

            if (direction2D == Vector2.zero)
                continue;

            direction2D.Normalize();

            float facadeWidth = facadePrefab.transform.localScale.x * length /
                                (length - length % facadePrefab.transform.localScale.x);

            if (length < facadePrefab.transform.localScale.x)
            {
                facadeWidth = length;
            }

            Vector3 point = new Vector3(pointA2D.x, y, pointA2D.y);
            Vector3 direction3D = new Vector3(direction2D.x, 0, direction2D.y);
            Vector3 leftDirection3D = new Vector3(direction2D.y, 0, -direction2D.x);
            Quaternion rotation = Quaternion.LookRotation(leftDirection3D);

            while (length > 0.01)
            {
                GameObject facade = Instantiate(facadePrefab, point + direction3D * (facadeWidth / 2), rotation);

                facade.transform.localScale = new Vector3(facadeWidth, facade.transform.localScale.y, facade.transform.localScale.z);
                facade.transform.parent = parent;

                length -= facadeWidth;
                point += direction3D * facadeWidth;
            }            
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
