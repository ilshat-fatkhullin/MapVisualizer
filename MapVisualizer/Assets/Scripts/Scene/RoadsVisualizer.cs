using System;
using System.Globalization;
using UnityEngine;

public class RoadsVisualizer : Visualizer
{
    private void Start()
    {
        BBox bBox = GeoPositioningHelper.GetBBoxFromLatitudeAndLongitude(OriginLatitude, OriginLongitude, Zoom);
        string request = BuildRequest(bBox);
        StartCoroutine(LoadFile(request));
    }

    private string BuildRequest(BBox bBox)
    {
        CultureInfo info = new CultureInfo("en-US");
        return string.Format(StringConstants.OSMTileRequestPattern,
            Convert.ToString(bBox.MinLongitude, info), Convert.ToString(bBox.MinLatitude, info),
            Convert.ToString(bBox.MaxLongitude, info), Convert.ToString(bBox.MaxLatitude, info));
    }

    protected override void OnNetworkResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Invalid response received");
        }

        Debug.Log(response);
    }
}
