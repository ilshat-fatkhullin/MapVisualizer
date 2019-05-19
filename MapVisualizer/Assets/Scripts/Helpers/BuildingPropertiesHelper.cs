using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuildingPropertiesHelper
{
    public static MeshInfo GetRoofInfo(BAMCIS.GeoJSON.Polygon polygon, IDictionary<string, dynamic> properties,
        Vector2 originInMeters)
    {
        float height = BuildingPropertiesHelper.GetHeightFromProperties(properties);

        PolygonLoops polygonLoops = GetPolygonLoopsInMeters(polygon, originInMeters);

        Vector2[] outerLoop = polygonLoops.OuterLoop;
        Vector2[][] holeLoops = polygonLoops.InnerLoop;        

        Sebastian.Geometry.Polygon polygon2D;
        if (holeLoops.Length == 0)
        {
            polygon2D = new Sebastian.Geometry.Polygon(outerLoop);
        }
        else
        {
            polygon2D = new Sebastian.Geometry.Polygon(outerLoop, holeLoops);
        }

        Sebastian.Geometry.Triangulator triangulator = new Sebastian.Geometry.Triangulator(polygon2D);
        int[] triangles = triangulator.Triangulate();
        Vector2[] vertices2D = polygon2D.points;
        Vector3[] vertices3D = new Vector3[vertices2D.Length * 2];

        int i = 0;
        foreach (var vertex2D in vertices2D)
        {
            vertices3D[i] = new Vector3(vertex2D.x, height, vertex2D.y);
            i++;
        }

        return new MeshInfo(vertices3D, triangles.ToArray());
    }

    public static MeshInfo GetWallInfo(BAMCIS.GeoJSON.Polygon polygon, IDictionary<string, dynamic> properties,
        Vector2 originInMeters)
    {
        float height = BuildingPropertiesHelper.GetHeightFromProperties(properties);

        PolygonLoops polygonLoops = GetPolygonLoopsInMeters(polygon, originInMeters);

        Vector2[] outerLoop = polygonLoops.OuterLoop;
        Vector2[][] holeLoops = polygonLoops.InnerLoop;

        List<int> triangles = new List<int>();

        List<Vector3> vertices = new List<Vector3>();

        foreach (var vertex2D in outerLoop)
        {
            vertices.Add(new Vector3(vertex2D.x, 0, vertex2D.y));
            vertices.Add(new Vector3(vertex2D.x, height, vertex2D.y));
        }

        for (int i = 0; i < outerLoop.Length; i++)
        {
            triangles.Add(i * 2);
            triangles.Add((i * 2 + 1) % vertices.Count);
            triangles.Add(((i + 1) * 2) % vertices.Count);

            triangles.Add((i * 2 + 1) % vertices.Count);
            triangles.Add(((i + 1) * 2 + 1) % vertices.Count);
            triangles.Add(((i + 1) * 2) % vertices.Count);
        }

        int offset = triangles.Count;

        foreach (var loop in holeLoops)
        {
            for (int i = loop.Length - 1; i >= 0; i++)
            {
                var vertex2D = loop[i];
                vertices.Add(new Vector3(vertex2D.x, 0, vertex2D.y));
                vertices.Add(new Vector3(vertex2D.x, height, vertex2D.y));
            }

            for (int i = offset; i < outerLoop.Length + offset; i++)
            {
                triangles.Add(i * 2);
                triangles.Add((i * 2 + 1) % vertices.Count);
                triangles.Add(((i + 1) * 2) % vertices.Count);

                triangles.Add((i * 2 + 1) % vertices.Count);
                triangles.Add(((i + 1) * 2 + 1) % vertices.Count);
                triangles.Add(((i + 1) * 2) % vertices.Count);
            }
        }

        return new MeshInfo(vertices.ToArray(), triangles.ToArray());
    }

    private static PolygonLoops GetPolygonLoopsInMeters(BAMCIS.GeoJSON.Polygon polygon, Vector2 originInMeters)
    {
        Vector2[] outerLoop = null;
        Vector2[][] holeLoops = new Vector2[polygon.Coordinates.Count() - 1][];

        bool isOuterLoop = true;

        int i = 0;
        foreach (var linearRing in polygon.Coordinates)
        {
            Vector2[] loop = new Vector2[linearRing.Coordinates.Count() - 1];

            int j = 0;

            foreach (var position in linearRing.Coordinates)
            {
                Vector2 positionInMeters = GeoPositioningHelper.GetMetersFromLatitudeAndLongitude(
                    position.Latitude, position.Longitude) - originInMeters;
                loop[j] = positionInMeters;
                j++;

                if (j == loop.Length)
                    break;
            }

            if (isOuterLoop)
            {
                outerLoop = loop;
            }
            else
            {
                holeLoops[i] = loop;
            }

            isOuterLoop = false;
            i++;
        }

        return new PolygonLoops(outerLoop, holeLoops);
    }

    private static float GetHeightFromProperties(IDictionary<string, dynamic> properties)
    {
        if (properties.ContainsKey("height"))
        {
            return (float)properties["height"];
        }

        if (properties.ContainsKey("levels"))
        {
            return GetHeightFromLevel((int)properties["levels"]);
        }

        Debug.Log("Building does not have any property to calculate its height.");
        return 0;
    }

    private static float GetHeightFromLevel(int level)
    {
        if (level <= 0)
        {
            return 0;
        }

        return NumericConstants.FIRST_LEVEL_HEIGHT + (level - 1) * NumericConstants.NON_FIRST_LEVEL_HEIGHT;
    }

    private class PolygonLoops
    {
        public Vector2[] OuterLoop { get; private set; }

        public Vector2[][] InnerLoop { get; private set; }

        public PolygonLoops(Vector2[] outerLoop, Vector2[][] innerLoop)
        {
            OuterLoop = outerLoop;
            InnerLoop = innerLoop;
        }
    }
}