using System;

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

    public string BuildRequest()
    {
        return string.Format(StringConstants.GeoJSONTileRequestPattern,
            Convert.ToString(Zoom), Convert.ToString(X), Convert.ToString(Y));
    }
}
