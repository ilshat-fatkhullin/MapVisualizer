using System.Collections.Generic;

public class Tile
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Zoom { get; private set; }

    public Tile(int x, int y, int zoom)
    {
        X = x;
        Y = y;
        Zoom = zoom;
    }

    public static bool operator ==(Tile a, Tile b)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        if (a.X == b.X && a.Y == b.Y && a.Zoom == b.Zoom)
            return true;
        return false;
    }

    public static bool operator !=(Tile a, Tile b)
    {
        if (a is null && b is null)
            return false;
        if (a is null || b is null)
            return true;
        if (a.X == b.X && a.Y == b.Y && a.Zoom == b.Zoom)
            return false;
        return true;
    }
}

public class TileEqualityComparer : IEqualityComparer<Tile>
{
    public bool Equals(Tile tile1, Tile tile2)
    {
        if (tile2 == null && tile1 == null)
            return true;
        else if (tile1 == null || tile2 == null)
            return false;
        else if (tile1.X == tile2.X && tile1.Y == tile2.Y && tile1.Zoom == tile2.Zoom)
            return true;
        else
            return false;
    }

    public int GetHashCode(Tile tile)
    {
        int hCode = tile.X ^ tile.Y ^ tile.Zoom;
        return hCode.GetHashCode();
    }
}
