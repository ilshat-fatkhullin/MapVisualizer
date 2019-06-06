using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Visualizer : MonoBehaviour
{
    public GameObject BuildingPrefab;

    public float OriginLatitude;

    public float OriginLongitude;

    public float DelayBetweenBatches;

    public int NumberOfObjectsInBatch;

    public int Zoom;

    protected Vector2 originInMeters;

    protected VisualizedTileMap map;

    protected Tile centerTile;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
        centerTile = new Tile(0, 0, 0);
        map = new VisualizedTileMap(centerTile);
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
                if (map.ObjectsToInstantiate.Count > 0)
                {
                    InstantiateObject(map.ObjectsToInstantiate.Dequeue());
                }
                if (map.ObjectsToDestroy.Count > 0)
                {
                    Destroy(map.ObjectsToDestroy.Dequeue());
                }
            }

            yield return new WaitForSeconds(DelayBetweenBatches);
        }
    }

    private void InstantiateObject(ObjectToInstantiate objectToInstantiate)
    {
        if (!map.ContainsTile(objectToInstantiate.Tile))
        {
            return;
        }

        GameObject building = Instantiate(BuildingPrefab);
        MeshFilter filter = building.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = objectToInstantiate.Info.Vertices;
        mesh.triangles = objectToInstantiate.Info.Triangles;
        mesh.SetUVs(0, new List<Vector2>(objectToInstantiate.Info.UVs));
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        filter.mesh = mesh;
        MeshRenderer renderer = building.GetComponent<MeshRenderer>();
        renderer.material = objectToInstantiate.Material;
        map.AddInstantiatedObjectToTile(objectToInstantiate.Tile, building);
    }

    protected abstract string BuildRequest(Tile tile);

    protected abstract void OnNetworkResponse(Tile tile, string response);   
}
