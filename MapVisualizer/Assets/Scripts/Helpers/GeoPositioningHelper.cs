using System;
using UnityEngine;

public static class GeoPositioningHelper
{
    public static Vector2 GetLongitudeAndLatitudeFromTile(float xTile, float yTile, int zoom)
    {
        double n = Math.PI - ((2.0 * Math.PI * yTile) / Math.Pow(2.0, zoom));
        return new Vector2(
            (float)((xTile / Math.Pow(2.0, zoom) * 360.0) - 180.0),
            (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)))
            );
    }

    public static BBox GetBBoxFromLatitudeAndLongitude(float latitudeDegrees, float longitudeDegrees, int zoom)
    {
        Point2D tile = GetTileFromLatitudeAndLongitude(latitudeDegrees, longitudeDegrees, zoom);

        float width = 425;

        float xMin = (tile.X * NumericConstants.TILE_SIZE - width / 2) / NumericConstants.TILE_SIZE;
        float yMin = (tile.Y * NumericConstants.TILE_SIZE - width / 2) / NumericConstants.TILE_SIZE;
        float xMax = (tile.X * NumericConstants.TILE_SIZE + width / 2) / NumericConstants.TILE_SIZE;
        float yMax = (tile.Y * NumericConstants.TILE_SIZE + width / 2) / NumericConstants.TILE_SIZE;

        Vector2 min = GetLongitudeAndLatitudeFromTile(xMin, yMin, zoom);
        Vector2 max = GetLongitudeAndLatitudeFromTile(xMax, yMax, zoom);

        return new BBox(min.x, min.y, max.x, max.y);
    }

    public static Point2D GetTileFromLatitudeAndLongitude(float latitudeDegrees, float longitudeDegrees, int zoom)
    {
        float latitudeRadians = latitudeDegrees * Mathf.Deg2Rad;
        float n = Mathf.Pow(2f, zoom);
        int xTile = Convert.ToInt32((longitudeDegrees + 180) / 360 * n);
        int yTile = Convert.ToInt32((1f - Mathf.Log(Mathf.Tan(latitudeRadians) + (1 / Mathf.Cos(latitudeRadians))) / Mathf.PI) / 2f * n);
        return new Point2D(xTile, yTile);
    }

    public static Vector2 GetMetersFromLatitudeAndLongitude(double latitudeDegrees, double longitudeDegrees)
    {
        double latitude = latitudeDegrees * Mathf.Deg2Rad;
        double longitude = longitudeDegrees * Mathf.Deg2Rad;
        double x = NumericConstants.EARTH_RADIUS * (Mathf.PI + longitude);
        double y = NumericConstants.EARTH_RADIUS * (Math.Log(Math.Tan(Math.PI / 4 + latitude / 2)) - Mathf.PI);
        return new Vector2((float)x, (float)y);
    }
}
