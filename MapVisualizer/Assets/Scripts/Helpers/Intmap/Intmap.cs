using System;
using UnityEngine;

public class Intmap
{
    public int[,] Map { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public Intmap(int width, int height)
    {
        Map = new int[width, height];
        Width = width;
        Height = height;
    }

    public void DrawLine(Point2D point1, Point2D point2, int value, int width)
    {
        int x0 = point1.X, x1 = point2.X, y0 = point1.Y, y1 = point2.Y;
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;
        int halfWidth = width / 2;
        for (; ; )
        {
            if (x0 >= 0 && x0 < Width && y0 >= 0 && y0 < Height)
            {                
                for (int i = x0 - halfWidth; i <= x0 + halfWidth; i++)
                    for (int j = y0 - halfWidth; j <= y0 + halfWidth; j++)
                    {
                        if (i >= 0 && i < Width && j >= 0 && j < Height)
                        {
                            Map[i, j] = value;
                        }
                    }
            }
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }    

    public void DrawFilledPolygon(Point2D[] points, int value)
    {
        DrawPolygon(points, value);

        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Max(Mathf.Min(minX, point.X), 0);
            minY = Mathf.Max(Mathf.Min(minY, point.Y), 0);
            maxX = Math.Min(Mathf.Max(maxX, point.X), Width - 1);
            maxY = Math.Min(Mathf.Max(maxY, point.Y), Height - 1);
        }

        for (int x = minX; x <= maxX; x++)
        {
            int y1 = maxY;
            int y2 = minY;

            for (int y = minY; y <= maxY; y++)
            {
                if (Map[x, y] == value)
                {
                    y1 = Mathf.Min(y, y1);
                    y2 = Mathf.Max(y, y1);
                }
            }

            DrawLine(new Point2D(x, y1), new Point2D(x, y2), value, 1);
        }
    }

    private void DrawPolygon(Point2D[] points, int value)
    {
        for (int i = 1; i < points.Length; i++)
        {
            DrawLine(points[i - 1], points[i], value, 1);
        }

        DrawLine(points[0], points[points.Length - 1], value, 1);
    }
}
