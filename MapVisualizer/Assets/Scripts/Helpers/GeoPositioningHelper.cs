using System;
using UnityEngine;

public static class GeoPositioningHelper
{
    public static Coordinate GetCoordinateFromTile(Tile tile)
    {
        double n = Math.PI - ((2.0 * Math.PI * tile.Y) / Math.Pow(2.0, tile.Zoom));
        return new Coordinate(
            (float)((tile.X / Math.Pow(2.0, tile.Zoom) * 360.0) - 180.0),
            (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)))
            );
    }

    public static BBox GetBBoxFromCoordinate(Coordinate coordinate, int zoom)
    {
        Tile tile = GetTileFromCoordinate(coordinate, zoom);

        float width = 325;

        int xA = Convert.ToInt32((tile.X * NumericConstants.TILE_SIZE - width / 2) / NumericConstants.TILE_SIZE);
        int yA = Convert.ToInt32((tile.Y * NumericConstants.TILE_SIZE - width / 2) / NumericConstants.TILE_SIZE);
        int xB = Convert.ToInt32((tile.X * NumericConstants.TILE_SIZE + width / 2) / NumericConstants.TILE_SIZE);
        int yB = Convert.ToInt32((tile.Y * NumericConstants.TILE_SIZE + width / 2) / NumericConstants.TILE_SIZE);

        Coordinate a = GetCoordinateFromTile(new Tile(xA, yA, zoom));
        Coordinate b = GetCoordinateFromTile(new Tile(xB, yB, zoom));

        return new BBox(Mathf.Min(a.Latitude, b.Latitude),
                        Mathf.Min(a.Longitude, b.Longitude),
                        Mathf.Max(a.Latitude, b.Latitude),
                        Mathf.Max(a.Longitude, b.Longitude));
    }

    public static Tile GetTileFromCoordinate(Coordinate coordinate, int zoom)
    {
        float latitudeRadians = coordinate.Latitude * Mathf.Deg2Rad;
        float n = Mathf.Pow(2f, zoom);
        int xTile = Convert.ToInt32((coordinate.Longitude + 180) / 360 * n);
        int yTile = Convert.ToInt32((1f - Mathf.Log(Mathf.Tan(latitudeRadians) + (1 / Mathf.Cos(latitudeRadians))) / Mathf.PI) / 2f * n);
        return new Tile(xTile, yTile, zoom);
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
