using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GeoJSONVisualizer : MonoBehaviour
{
    public GameObject BuildingPrefab;

    public int Zoom;

    public float OriginLatitude;

    public float OriginLongitude;

    private FeatureCollection tile;

    private Vector2 originInMeters;

    private void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromLatitudeAndLongitude(OriginLatitude, OriginLongitude);
        LoadGeoJSON();
    }

    private string BuildGeoJSONRequest(int x, int y, int z)
    {
        return string.Format(StringConstants.GeoJSONTileRequestPattern,
            Convert.ToString(x), Convert.ToString(y), Convert.ToString(z));
    }

    private void LoadGeoJSON()
    {
        Point2D tile = GeoPositioningHelper.GetTileFromLatitudeAndLongitude(OriginLatitude, OriginLongitude, Zoom);
        string request = BuildGeoJSONRequest(Zoom, tile.X, tile.Y);
        StartCoroutine(HttpGetRequest(request));
    }

    IEnumerator HttpGetRequest(string request)
    {
        UnityWebRequest www = UnityWebRequest.Get(request);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            OnNetworkResponse(www.downloadHandler.text);
        }
    }

    private void OnNetworkResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Invalid response received");
        }

        tile = JsonConvert.DeserializeObject<FeatureCollection>(response);

        foreach (var feature in tile.Features)
        {
            InstantiateGeometry(feature.Geometry, feature.Properties);
        }
    }

    private void InstantiateGeometry(Geometry geometry, IDictionary<string, dynamic> properties)
    {
        switch (geometry.Type)
        {
            case GeoJsonType.Point:
                InstantiatePoint(geometry as Point);
                break;
            case GeoJsonType.MultiPoint:
                InstantiateMultiPoint(geometry as MultiPoint);
                break;
            case GeoJsonType.LineString:
                InstantiateLineString(geometry as LineString);
                break;
            case GeoJsonType.MultiLineString:
                InstantiateMultiLineString(geometry as MultiLineString);
                break;
            case GeoJsonType.Polygon:
                InstantiatePolygon(geometry as Polygon, properties);
                break;
            case GeoJsonType.MultiPolygon:
                InstantiateMultiPolygon(geometry as MultiPolygon);
                break;
            case GeoJsonType.GeometryCollection:
                InstantiateGeometryCollection(geometry as GeometryCollection, properties);
                break;
            default:
                Debug.LogError("Invalid type of geometry to be instantiated");
                break;
        }
    }

    private void InstantiatePoint(Point point)
    {

    }

    private void InstantiateMultiPoint(MultiPoint multiPoint)
    {

    }

    private void InstantiateLineString(LineString lineString)
    {

    }

    private void InstantiateMultiLineString(MultiLineString multiLineString)
    {

    }

    private void InstantiatePolygon(Polygon polygon, IDictionary<string, dynamic> properties)
    {
        float height = BuildingPropertiesHelper.GetHeightFromProperties(properties);

        List<Vector3> vertices = new List<Vector3>();

        bool isFirst = true;

        foreach (var linearRing in polygon.Coordinates)
        {
            if (!isFirst)
                break;

            foreach (var position in linearRing.Coordinates)
            {
                Vector2 positionInMeters = GeoPositioningHelper.GetMetersFromLatitudeAndLongitude(
                    position.Latitude, position.Longitude) - originInMeters;

                vertices.Add(new Vector3(positionInMeters.x, 0, positionInMeters.y));
                vertices.Add(new Vector3(positionInMeters.x, height, positionInMeters.y));
            }

            isFirst = false;
        }

        List<int> triangles = new List<int>();

        for (int i = 0; i < vertices.Count - 3; i += 2)
        {
            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i);

            triangles.Add(i + 1);
            triangles.Add(i + 3);
            triangles.Add(i + 2);                        
        }

        for (int i = 3; i + 2 < vertices.Count; i += 2)
        {
            triangles.Add(1);
            triangles.Add(i + 2);
            triangles.Add(i);            
        }

        for (int i = 2; i + 2 < vertices.Count; i += 2)
        {
            triangles.Add(0);
            triangles.Add(i + 2);
            triangles.Add(i);            
        }

        InstantiateObject(vertices.ToArray(), triangles.ToArray());
    }

    private void InstantiateMultiPolygon(MultiPolygon multiPolygon)
    {

    }

    private void InstantiateGeometryCollection(GeometryCollection geometryCollection, IDictionary<string, dynamic> properties)
    {
        foreach (var g in geometryCollection.Geometries)
        {
            InstantiateGeometry(g, properties);
        }
    }


    private void InstantiateObject(Vector3[] vertices, int[] triangles)
    {
        GameObject building = Instantiate(BuildingPrefab);
        MeshFilter filter = building.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        filter.mesh = mesh;
    }
}
