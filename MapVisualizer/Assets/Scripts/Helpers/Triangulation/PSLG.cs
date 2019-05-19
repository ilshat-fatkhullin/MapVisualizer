using System.Collections.Generic;
using UnityEngine;

public class PSLG
{
    public List<Vector2> vertices;
    public List<int[]> segments;
    public List<PSLG> holes;
    public List<int> boundaryMarkersForPolygons;

    public PSLG()
    {
        vertices = new List<Vector2>();
        segments = new List<int[]>();
        holes = new List<PSLG>();
        boundaryMarkersForPolygons = new List<int>();
    }

    public PSLG(List<Vector2> vertices) : this()
    {
        this.AddVertexLoop(vertices);
    }

    public void AddVertexLoop(List<Vector2> vertices)
    {
        if (vertices.Count < 3)
        {
            Debug.Log("A vertex loop cannot have less than three vertices " + vertices.Count);
        }
        else
        {
            this.vertices.AddRange(vertices);
            int segmentOffset = segments.Count;
            boundaryMarkersForPolygons.Add(segments.Count);
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                segments.Add(new int[] { i + segmentOffset, i + 1 + segmentOffset });
            }
            segments.Add(new int[] { vertices.Count - 1 + segmentOffset, segmentOffset });
        }
    }

    public void AddOrderedVertices(Vector2[] vertices)
    {
        AddVertexLoop(new List<Vector2>(vertices));
    }

    public void AddHole(List<Vector2> vertices)
    {
        PSLG hole = new PSLG();
        hole.AddVertexLoop(vertices);
        holes.Add(hole);
    }

    public void AddHole(Vector2[] vertices)
    {
        AddHole(new List<Vector2>(vertices));
    }

    public int GetNumberOfSegments()
    {
        int offset = vertices.Count;
        foreach (PSLG hole in holes)
        {
            offset += hole.segments.Count;
        }

        return offset;
    }

    public bool IsPointInPolygon(Vector2 point)
    {
        int j = segments.Count - 1;
        bool oddNodes = false;

        for (int i = 0; i < segments.Count; i++)
        {
            if ((vertices[i].y < point.y && vertices[j].y >= point.y
            || vertices[j].y < point.y && vertices[i].y >= point.y)
            && (vertices[i].x <= point.x || vertices[j].x <= point.x))
            {
                oddNodes ^= (vertices[i].x + (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) * (vertices[j].x - vertices[i].x) < point.x);
            }
            j = i;
        }

        return oddNodes;
    }

    public Vector2 GetPointInPolygon()
    {
        float topMost = vertices[0].y;
        float bottomMost = vertices[0].y;
        float leftMost = vertices[0].x;
        float rightMost = vertices[0].x;

        foreach (Vector2 vertex in vertices)
        {
            if (vertex.y > topMost)
                topMost = vertex.y;
            if (vertex.y < bottomMost)
                bottomMost = vertex.y;
            if (vertex.x < leftMost)
                leftMost = vertex.x;
            if (vertex.x > rightMost)
                leftMost = vertex.x;
        }

        Vector2 point;

        int whileCount = 0;
        do
        {
            point = new Vector2(Random.Range(leftMost, rightMost), Random.Range(bottomMost, topMost));
            whileCount++;
            if (whileCount > 10000)
            {
                string polygonstring = "";
                foreach (Vector2 vertex in vertices)
                {
                    polygonstring += vertex + ", ";
                }
                Debug.LogError("Stuck in while loop. Vertices: " + polygonstring);
                break;
            }
        }
        while (!IsPointInPolygon(point));

        return point;
    }

    public Vector2 GetPointInHole(PSLG hole)
    {
        // 10 Get point in hole
        // 20 Is the point in a polygon that the hole is not in
        // 30 if so goto 10 else return
        List<PSLG> polygons = new List<PSLG>();
        for (int i = 0; i < boundaryMarkersForPolygons.Count; i++)
        {
            int startIndex = boundaryMarkersForPolygons[i];
            int endIndex = vertices.Count - 1;
            if (i < boundaryMarkersForPolygons.Count - 1)
                endIndex = boundaryMarkersForPolygons[i + 1] - 1;
            polygons.Add(new PSLG(vertices.GetRange(startIndex, endIndex - startIndex + 1)));
        }

        int whileCount = 0;

        Vector2 point;
        bool isPointGood;
        do
        {

            isPointGood = true;
            point = hole.GetPointInPolygon();
            foreach (PSLG polygon in polygons)
            {
                string polygonVertices = "";
                foreach (Vector2 vertex in polygon.vertices)
                    polygonVertices += vertex + ",";

                if (polygon.IsPointInPolygon(hole.vertices[0]))
                {
                    // This polygon surrounds the hole, which is OK
                }
                else if (hole.IsPointInPolygon(polygon.vertices[0]))
                {
                    // This polygon is within the hole

                    if (polygon.IsPointInPolygon(point))
                    {
                        // The point is within a polygon that is inside the hole, which is NOT OK
                        isPointGood = false;
                    }
                    else
                    {
                        // But the point was not within this polygon
                    }
                }
                else
                {
                    // This polygon is far away from the hole
                }

            }
            whileCount++;
            if (whileCount > 10000)
            {
                string holestring = "";
                foreach (Vector2 vertex in hole.vertices)
                {
                    holestring += vertex + ", ";
                }

                Debug.LogError("Stuck in while loop. Hole vertices: " + holestring);
                break;
            }
        }
        while (!isPointGood);

        return point;
    }
}