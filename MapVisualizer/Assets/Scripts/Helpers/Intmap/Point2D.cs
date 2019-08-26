using System.Collections.Generic;

public struct Point2D
{
    public int X
    {
        get;
        set;
    }

    public int Y
    {
        get;
        set;
    }

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Point2DEqualityComparer : IEqualityComparer<Point2D>
{
    public bool Equals(Point2D x, Point2D y)
    {
        return x.X == y.X && x.Y == y.Y;
    }

    public int GetHashCode(Point2D obj)
    {
        return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
    }
}