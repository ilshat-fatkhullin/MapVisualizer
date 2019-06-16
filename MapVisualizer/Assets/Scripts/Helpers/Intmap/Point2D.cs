using System.Collections.Generic;

public class Point2D
{
    public int X
    {
        get;
        private set;
    }

    public int Y
    {
        get;
        private set;
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
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.X == y.X && x.Y == y.Y;
    }

    public int GetHashCode(Point2D obj)
    {
        if (obj == null)
        {
            return 0;
        }

        return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
    }
}