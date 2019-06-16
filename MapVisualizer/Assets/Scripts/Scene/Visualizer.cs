using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <typeparam name="O">Data, which stores all info about how to instantiate an object.</typeparam>
public abstract class Visualizer: MonoBehaviour
{
    public GameObject BuildingPrefab;

    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

    protected Vector2 originInMeters;

    protected Tile currentTile;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
        currentTile = new Tile(0, 0, 0);
    }

    private void Update()
    {
        Vector2 cameraPositionInMeters = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        cameraPositionInMeters += originInMeters;
        cameraPositionInMeters += new Vector2(-NumericConstants.TILE_SIZE, NumericConstants.TILE_SIZE);

        Tile currentTile = GeoPositioningHelper.GetTileFromCoordinate(
            GeoPositioningHelper.GetCoordinateFromMeters(cameraPositionInMeters),
            Zoom);

        if (this.currentTile != currentTile)
        {
            this.currentTile = currentTile;
            VisualizeTile(currentTile);
        }
    }

    private void VisualizeTile(Tile tile)
    {
        StartCoroutine(LoadFile(tile, BuildRequest(tile)));
    }

    private IEnumerator LoadFile(Tile tile, string request)
    {
        UnityWebRequest www = UnityWebRequest.Get(request);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            if (www.isHttpError)
            {
                Debug.LogError("Invalid response received");
                yield return null;
            }            
            OnNetworkResponse(tile, www.downloadHandler.text);
        }
    }

    protected abstract string BuildRequest(Tile tile);

    protected abstract void OnNetworkResponse(Tile tile, string response);

    private void OnDrawGizmos()
    {
        if (currentTile == null)
            return;

        Gizmos.color = Color.red;

        Vector2 point1 = GeoPositioningHelper.GetMetersFromCoordinate(
                GeoPositioningHelper.GetNWCoordinateFromTile(currentTile)) - originInMeters;

        Vector2 point2 = GeoPositioningHelper.GetMetersFromCoordinate(
                GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(currentTile.X + 1, currentTile.Y + 1, currentTile.Zoom))) - originInMeters;

        Vector2 point3 = GeoPositioningHelper.GetMetersFromCoordinate(
                GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(currentTile.X + 1, currentTile.Y, currentTile.Zoom))) - originInMeters;

        Vector2 point4 = GeoPositioningHelper.GetMetersFromCoordinate(
                GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(currentTile.X, currentTile.Y + 1, currentTile.Zoom))) - originInMeters;

        Gizmos.DrawLine(new Vector3(point1.x, 10, point1.y), new Vector3(point2.x, 10, point2.y));
        Gizmos.DrawLine(new Vector3(point1.x, 10, point1.y), new Vector3(point3.x, 10, point3.y));
        Gizmos.DrawLine(new Vector3(point1.x, 10, point1.y), new Vector3(point4.x, 10, point4.y));
        Gizmos.DrawLine(new Vector3(point2.x, 10, point2.y), new Vector3(point3.x, 10, point3.y));
        Gizmos.DrawLine(new Vector3(point2.x, 10, point2.y), new Vector3(point4.x, 10, point4.y));
    }
}
