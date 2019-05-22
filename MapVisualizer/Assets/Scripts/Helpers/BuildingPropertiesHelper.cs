using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuildingPropertiesHelper
{
    public static MeshInfo GetRoofInfo(PolygonLoops polygonLoops, IDictionary<string, dynamic> properties)
    {
        float height = BuildingPropertiesHelper.GetHeightFromProperties(properties);

        Vector2[] outerLoop = polygonLoops.OuterLoop;
        Vector2[][] holeLoops = polygonLoops.InnerLoops;        

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

        Vector3[] uvs = new Vector3[vertices3D.Length];
        for (int k = 0; k < uvs.Length; k++)
        {
            uvs[k] = new Vector3(vertices3D[k].x, vertices3D[k].z, 0);
        }

        return new MeshInfo(vertices3D, triangles.ToArray(), uvs);
    }

    public static MeshInfo GetWallInfo(PolygonLoops polygonLoops, IDictionary<string, dynamic> properties)
    {
        float height = BuildingPropertiesHelper.GetHeightFromProperties(properties);

        Vector2[] outerLoop = polygonLoops.OuterLoop;
        Vector2[][] holeLoops = polygonLoops.InnerLoops;

        List<int> triangles = new List<int>();

        List<Vector3> vertices = new List<Vector3>();

        foreach (var vertex2D in outerLoop)
        {
            vertices.Add(new Vector3(vertex2D.x, 0, vertex2D.y));
            vertices.Add(new Vector3(vertex2D.x, height, vertex2D.y));
        }

        int size = outerLoop.Length * 2;

        for (int i = 0; i < outerLoop.Length; i++)
        {
            triangles.Add(i * 2);
            triangles.Add((i * 2 + 1) % size);
            triangles.Add(((i + 1) * 2) % size);

            triangles.Add((i * 2 + 1) % size);
            triangles.Add(((i + 1) * 2 + 1) % size);
            triangles.Add(((i + 1) * 2) % size);
        }

        int offset = vertices.Count;

        foreach (var loop in holeLoops)
        {
            foreach (var vertex2D in loop)
            {
                vertices.Add(new Vector3(vertex2D.x, 0, vertex2D.y));
                vertices.Add(new Vector3(vertex2D.x, height, vertex2D.y));
            }

            size = loop.Length * 2;

            for (int i = 0; i < loop.Length; i++)
            {
                triangles.Add(offset + (i * 2));
                triangles.Add(offset + ((i * 2 + 1) % size));
                triangles.Add(offset + (((i + 1) * 2) % size));

                triangles.Add(offset + ((i * 2 + 1) % size));
                triangles.Add(offset + (((i + 1) * 2 + 1) % size));
                triangles.Add(offset + (((i + 1) * 2) % size));                                
            }
        }

        return new MeshInfo(vertices.ToArray(), triangles.ToArray(), vertices.ToArray());
    }

    public static PolygonLoops GetPolygonLoopsInMeters(BAMCIS.GeoJSON.Polygon polygon, Vector2 originInMeters)
    {
        Vector2[] outerLoop = null;
        Vector2[][] holeLoops = new Vector2[polygon.Coordinates.Count() - 1][];

        bool isOuterLoop = true;

        int i = 0;
        foreach (var linearRing in polygon.Coordinates)
        {
            List<Vector2> loop = new List<Vector2>(linearRing.Coordinates.Count() - 1);

            foreach (var position in linearRing.Coordinates)
            {
                Vector2 positionInMeters = GeoPositioningHelper.GetMetersFromLatitudeAndLongitude(
                    position.Latitude, position.Longitude) - originInMeters;
                loop.Add(positionInMeters);
            }

            loop = loop.Distinct().Reverse().ToList();

            if (isOuterLoop)
            {
                outerLoop = loop.ToArray();
            }
            else
            {
                holeLoops[i - 1] = loop.ToArray();
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
        return NumericConstants.FIRST_LEVEL_HEIGHT;
    }

    private static float GetHeightFromLevel(int level)
    {
        if (level <= 0)
        {
            return 0;
        }

        return NumericConstants.FIRST_LEVEL_HEIGHT + (level - 1) * NumericConstants.NON_FIRST_LEVEL_HEIGHT;
    }

    public class PolygonLoops
    {
        public Vector2[] OuterLoop { get; private set; }

        public Vector2[][] InnerLoops { get; private set; }

        public PolygonLoops(Vector2[] outerLoop, Vector2[][] innerLoops)
        {
            OuterLoop = outerLoop;
            InnerLoops = innerLoops;
        }
    }

    private class Vector2EqualityComparer : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 v1, Vector2 v2)
        {
            if (v2 == null && v1 == null)
                return true;
            else if (v1 == null || v2 == null)
                return false;
            else if (v1.x == v2.x && v1.y == v2.y)
                return true;
            else
                return false;
        }

        public int GetHashCode(Vector2 v)
        {
            return (int)v.magnitude;
        }
    }
}