﻿using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GeoJSONVisualizer : MonoBehaviour
{
    public GameObject BuildingPrefab;

    public Material WallMaterial;

    public Material RoofMaterial;

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
        MeshInfo roofInfo = BuildingPropertiesHelper.GetRoofInfo(polygon, properties, originInMeters);
        MeshInfo wallInfo = BuildingPropertiesHelper.GetWallInfo(polygon, properties, originInMeters);

        InstantiateObject(roofInfo, RoofMaterial);
        InstantiateObject(wallInfo, WallMaterial);
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

    private void InstantiateObject(MeshInfo info, Material material)
    {
        GameObject building = Instantiate(BuildingPrefab);
        MeshFilter filter = building.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = info.Vertices;
        mesh.triangles = info.Triangles;
        mesh.SetUVs(0, new List<Vector3>(info.UVs));
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        filter.mesh = mesh;
        MeshRenderer renderer = building.GetComponent<MeshRenderer>();
        renderer.material = material;
    }
}
