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
