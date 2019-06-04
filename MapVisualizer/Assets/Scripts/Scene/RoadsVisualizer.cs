using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class RoadsVisualizer : Visualizer
{
    public Material AsphaultMaterial;

    private Dictionary<string, Vector2> nodes;

    private CultureInfo info = new CultureInfo("en-US");

    protected override void Start()
    {
        base.Start();
        BBox bBox = GeoPositioningHelper.GetBBoxFromCoordinate(
            new Coordinate(
            OriginLatitude,
            OriginLongitude
            ), Zoom);
        string request = BuildRequest(bBox);
        StartCoroutine(LoadFile(request));
    }

    private string BuildRequest(BBox bBox)
    {
        return string.Format(StringConstants.OSMTileRequestPattern,
            Convert.ToString(bBox.MinLongitude, info), Convert.ToString(bBox.MinLatitude, info),
            Convert.ToString(bBox.MaxLongitude, info), Convert.ToString(bBox.MaxLatitude, info));
    }

    protected override void OnNetworkResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Invalid response received");
        }

        nodes = new Dictionary<string, Vector2>();

        XmlDocument document = new XmlDocument();
        document.LoadXml(response);
        XmlElement root = document.DocumentElement;

        foreach (XmlNode node in root)
        {
            if (node.Name == "node")
            {                

                Vector2 coordinates = GeoPositioningHelper.GetMetersFromCoordinate(
                    new Coordinate(
                    Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value, info),
                    Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value, info)
                    )) - originInMeters;

                nodes.Add(node.Attributes.GetNamedItem("id").Value, coordinates);
            }
        }

        foreach (XmlNode node in root)
        {
            if (node.Name == "way")
            {
                List<Vector2> points = new List<Vector2>();
                int lanes = 1;

                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == "nd")
                    {
                        points.Add(nodes[subNode.Attributes.GetNamedItem("ref").Value]);
                    }
                    else if (subNode.Name == "tag" && subNode.Attributes.GetNamedItem("k").Value == "lanes")
                    {
                        lanes = Convert.ToInt32(subNode.Attributes.GetNamedItem("v").Value);
                    }
                }

                Road road = new Road(lanes, points.ToArray());
                InstantiateRoad(road);
            }
        }
    }

    private void InstantiateRoad(Road road)
    {
        Vector2[] splinePoints = SplineBuilder.GetSplinePoints(road.Points, NumericConstants.ROAD_SPLINE_STEP);

        Vector3[] vertices = new Vector3[splinePoints.Length * 2];
        List<int> triangles = new List<int>(splinePoints.Length * 3);
        Vector2[] uv = new Vector2[vertices.Length];

        float width = NumericConstants.ROAD_LANE_WIDTH * road.Lanes;

        Vector2 direction = Vector2.zero;
        Vector2 right;
        Vector2 point;
        Vector2 rightPoint;
        Vector2 leftPoint;

        for (int i = 0; i < splinePoints.Length; i++)
        {
            point = splinePoints[i];

            if (i + 1 != splinePoints.Length)
            {
                direction = (splinePoints[i + 1] - point).normalized;
            }

            right = Vector2DHelper.Rotate2D(direction, 90);
            rightPoint = point + right * (width / 2);
            leftPoint = point - right * (width / 2);

            vertices[i * 2] = new Vector3(rightPoint.x, NumericConstants.ROAD_Y_OFFSET, rightPoint.y);
            vertices[i * 2 + 1] = new Vector3(leftPoint.x, NumericConstants.ROAD_Y_OFFSET, leftPoint.y);
        }

        for (int i = 0; i < vertices.Length - 2; i += 2)
        {
            triangles.Add(i + 3);
            triangles.Add(i + 1);
            triangles.Add(i);

            triangles.Add(i + 2);
            triangles.Add(i + 3);
            triangles.Add(i);                        
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        MeshInfo meshInfo = new MeshInfo(vertices, triangles.ToArray(), uv);
        InstantiateObject(meshInfo, AsphaultMaterial);
    }
}
