using System;

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
        for (; ; )
        {
            if (x0 >= 0 && x0 < Width && y0 >= 0 && y0 < Height)
            {
                for (int i = x0 - width + 1; i < x0 + width; i++)
                    for (int j = y0 - width + 1; j < y0 + width; j++)
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
}
