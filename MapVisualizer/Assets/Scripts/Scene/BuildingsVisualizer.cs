using BAMCIS.GeoJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsVisualizer : Singleton<BuildingsVisualizer>
{
    public GameObject Roof;

    public Material RoofMaterial;

    public GameObject FacadeContainer;

    public GameObject[] BaseFacades;

    public GameObject[] MiddleFacades;    

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
        PolygonLoops polygonLoops = BuildingPropertiesHelper.GetPolygonLoopsInMeters(polygon, originInMeters);

        float height = 0;
        InstantiateWalls(polygonLoops.OuterLoop, BuildingPropertiesHelper.GetNumberOfLevels(properties), out height);
        InstantiateRoof(BuildingPropertiesHelper.GetRoofInfo(polygonLoops, height), RoofMaterial);
    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties, Tile tile,
        Vector2 originInMeters)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties, tile, originInMeters);
        }
    }

    private void InstantiateWalls(Vector2[] points, int levels, out float y)
    {
        y = 0;
        for (int i = 0; i < levels; i++)
        {
            GameObject facade;
            if (i == 0 && levels > 1)
            {
                facade = BaseFacades[UnityEngine.Random.Range(0, BaseFacades.Length)];
            }
            else
            {
                facade = MiddleFacades[UnityEngine.Random.Range(0, MiddleFacades.Length)];
            }
            InstantiateFacade(points, y + facade.transform.lossyScale.y / 2, facade);
            y += facade.transform.lossyScale.y;
        }
    }

    private void InstantiateFacade(Vector2[] points, float y, GameObject facadePrefab)
    {
        GameObject facadeContainer = Instantiate(FacadeContainer);
        MeshRenderer facadeContainerRenderer = facadeContainer.GetComponent<MeshRenderer>();

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
                facade.transform.parent = facadeContainer.transform;
                facadeContainerRenderer.materials = facade.GetComponent<MeshRenderer>().materials;

                length -= facadeWidth;
                point += direction3D * facadeWidth;
            }            
        }

        buildings.Add(facadeContainer);
    }

    private void InstantiateRoof(MeshInfo info, Material material)
    {
        GameObject building = Instantiate(Roof);
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
