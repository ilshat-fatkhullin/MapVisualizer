using System;
using UnityEngine;

public static class GeoPositioningHelper
{
    /// <summary>
    /// Returns NW corner coordinate of the tile.
    /// </summary>
    public static Coordinate GetNWCoordinateFromTile(Tile tile)
    {
        double n = Math.PI - ((2.0 * Math.PI * tile.Y) / Math.Pow(2.0, tile.Zoom));
        return new Coordinate(
            (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n))),
            (float)((tile.X / Math.Pow(2.0, tile.Zoom) * 360.0) - 180.0)
            );
    }

    public static Tile GetTileFromCoordinate(Coordinate coordinate, int zoom)
    {
        float latitudeRadians = coordinate.Latitude * Mathf.Deg2Rad;
        float n = Mathf.Pow(2f, zoom);
        int xTile = Mathf.RoundToInt((coordinate.Longitude + 180) / 360 * n);
        int yTile = Mathf.RoundToInt((1f - Mathf.Log(Mathf.Tan(latitudeRadians) + (1 / Mathf.Cos(latitudeRadians))) / Mathf.PI) / 2f * n);
        return new Tile(xTile, yTile , zoom);
    }

    public static BBox GetBBoxFromTile(Tile tile)
    {        
        Coordinate p1 = GetNWCoordinateFromTile(tile);
        Coordinate p2 = GetNWCoordinateFromTile(new Tile(tile.X + 1, tile.Y + 1, tile.Zoom));
        return new BBox(
            Mathf.Min(p2.Longitude, p1.Longitude),
            Mathf.Min(p2.Latitude, p1.Latitude),
            Mathf.Max(p2.Longitude, p1.Longitude),
            Mathf.Max(p2.Latitude, p1.Latitude));
    }

    public static Vector2 GetMetersFromCoordinate(Coordinate coordinate)
    {
        float latitude = coordinate.Latitude * Mathf.Deg2Rad;
        float longitude = coordinate.Longitude * Mathf.Deg2Rad;
        float x = NumericConstants.EARTH_RADIUS * longitude;
        float y = NumericConstants.EARTH_RADIUS * Mathf.Log(Mathf.Tan(Mathf.PI / 4 + latitude / 2));
        return new Vector2(x, y);
    }

    public static Coordinate GetCoordinateFromMeters(Vector2 meters)
    {
        float longitude = meters.x / NumericConstants.EARTH_RADIUS;
        float latitude = (Mathf.Atan(Mathf.Exp(meters.y / NumericConstants.EARTH_RADIUS)) - Mathf.PI / 4) * 2;
        return new Coordinate(latitude * Mathf.Rad2Deg, longitude * Mathf.Rad2Deg);
    }
}
