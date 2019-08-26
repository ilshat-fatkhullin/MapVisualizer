using System.Collections.Generic;

public struct Tile
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

    public static bool operator !=(Tile a, Tile b)
    {
        return !(a == b);
    }

    public static bool operator ==(Tile a, Tile b)
    {
        return a.X == b.X && a.Y == b.Y && a.Zoom == b.Zoom;
    }
}

public class TileEqualityComparer : IEqualityComparer<Tile>
{
    public bool Equals(Tile tile1, Tile tile2)
    {
        return tile1.X == tile2.X && tile1.Y == tile2.Y && tile1.Zoom == tile2.Zoom;
    }

    public int GetHashCode(Tile tile)
    {
        return tile.X.GetHashCode() ^ tile.Y.GetHashCode() ^ tile.Zoom.GetHashCode();
    }
}
