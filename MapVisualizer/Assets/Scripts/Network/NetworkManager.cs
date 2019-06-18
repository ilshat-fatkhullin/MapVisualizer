using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkManager: Singleton<NetworkManager>
{
    public NetworkEvent OnDowloaded;

    public enum RequestType { OsmFile, GeoJSON }

    private void Awake()
    {
        OnDowloaded = new NetworkEvent();
    }

    public void DownloadTile(Tile tile, RequestType requestType)
    {
        StartCoroutine(DownloadFile(tile, requestType));
    }

    private IEnumerator DownloadFile(Tile tile, RequestType requestType)
    {
        string request = null;

        switch (requestType)
        {
            case RequestType.OsmFile:
                request = BuildOsmFileRequest(tile);
                break;
            case RequestType.GeoJSON:
                request = BuildGeoJSONRequest(tile);
                break;
        }

        UnityWebRequest www = UnityWebRequest.Get(request);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            yield return null;
        }

        if (www.isHttpError)
        {
            Debug.LogError("Invalid response received");
            yield return null;
        }

        OnDowloaded.Invoke(tile, requestType, www.downloadHandler.text);
    }

    private string BuildGeoJSONRequest(Tile tile)
    {
        return string.Format(StringConstants.GeoJSONTileRequestPattern,
            Convert.ToString(tile.Zoom), Convert.ToString(tile.X), Convert.ToString(tile.Y));
    }

    private string BuildOsmFileRequest(Tile tile)
    {
        BBox bBox = GeoPositioningHelper.GetBBoxFromTile(tile);
        return string.Format(StringConstants.OSMTileRequestPattern,
            Convert.ToString(bBox.MinLongitude, CultureInfoHelper.EnUSInfo),
            Convert.ToString(bBox.MinLatitude, CultureInfoHelper.EnUSInfo),
            Convert.ToString(bBox.MaxLongitude, CultureInfoHelper.EnUSInfo),
            Convert.ToString(bBox.MaxLatitude, CultureInfoHelper.EnUSInfo));
    }
}

public class NetworkEvent : UnityEvent<Tile, NetworkManager.RequestType, string>
{

}
