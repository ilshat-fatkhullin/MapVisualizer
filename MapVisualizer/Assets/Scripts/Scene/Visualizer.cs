using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <typeparam name="O">Data, which stores all info about how to instantiate an object.</typeparam>
public abstract class Visualizer<O> : MonoBehaviour where O : ObjectToInstantiate
{
    public GameObject BuildingPrefab;

    public float OriginLatitude;

    public float OriginLongitude;

    public float DelayBetweenBatches;

    public int NumberOfObjectsInBatch;

    public int Zoom;

    protected Vector2 originInMeters;

    protected VisualizedTileMap<O> map;

    protected Tile centerTile;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
        centerTile = new Tile(0, 0, 0);
        map = new VisualizedTileMap<O>(centerTile);
        StartCoroutine(ManageObjects());
    }

    private void Update()
    {
        //Checking if camera moved to another tile.

        Vector2 cameraPositionInMeters = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        cameraPositionInMeters += originInMeters;

        Tile currentTile = GeoPositioningHelper.GetTileFromCoordinate(
            GeoPositioningHelper.GetCoordinateFromMeters(cameraPositionInMeters),
            Zoom);

        if (centerTile != currentTile)
        {
            centerTile = currentTile;
            map.UpdateCenterTile(centerTile);

            foreach (var tileToVisualize in map.TilesToVisualize)
            {
                VisualizeTile(tileToVisualize);
            }
            map.TilesToVisualize.Clear();          
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

    private IEnumerator ManageObjects()
    {
        while (true)
        {
            for (int i = 0; i < NumberOfObjectsInBatch; i++)
            {
                if (map.HasObjectsToInstatiate())
                {
                    ObjectToInstantiate objectToInstantiate = map.DequeueObjectToInstantiate();

                    if (map.ContainsTile(objectToInstantiate.Tile))
                    {
                        GameObject instantiatedObject = InstantiateObject(objectToInstantiate as O);
                        map.AddInstantiatedObjectToTile(objectToInstantiate.Tile, instantiatedObject);
                    }
                }
                if (map.HasObjectsToDestroy())
                {
                    Destroy(map.DequeueObjectToDestroy());
                }
            }

            yield return new WaitForSeconds(DelayBetweenBatches);
        }
    }

    protected abstract GameObject InstantiateObject(O objectToInstantiate);

    protected abstract string BuildRequest(Tile tile);

    protected abstract void OnNetworkResponse(Tile tile, string response);   
}
