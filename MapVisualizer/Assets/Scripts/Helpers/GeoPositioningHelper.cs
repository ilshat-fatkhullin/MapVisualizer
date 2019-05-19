using System;
using UnityEngine;

public static class GeoPositioningHelper
{
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
        double y = NumericConstants.EARTH_RADIUS * (Mathf.PI - Math.Log(Math.Tan(Math.PI / 4 + latitude / 2)));
        return new Vector2((float)x, (float)y);
    }
}
