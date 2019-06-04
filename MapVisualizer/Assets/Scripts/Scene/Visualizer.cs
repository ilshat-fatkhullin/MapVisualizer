using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Visualizer : MonoBehaviour
{
    public GameObject BuildingPrefab;

    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

    protected Vector2 originInMeters;

    protected virtual void Start()
    {
        originInMeters = GeoPositioningHelper.GetMetersFromCoordinate(new Coordinate(OriginLatitude, OriginLongitude));
    }

    public IEnumerator LoadFile(string request)
    {
        UnityWebRequest www = UnityWebRequest.Get(request);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            OnNetworkResponse(www.downloadHandler.text);
        }
    }

    protected abstract void OnNetworkResponse(string response);

    protected void InstantiateObject(MeshInfo info, Material material)
    {
        GameObject building = Instantiate(BuildingPrefab);
        MeshFilter filter = building.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = info.Vertices;
        mesh.triangles = info.Triangles;
        mesh.SetUVs(0, new List<Vector2>(info.UVs));
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        filter.mesh = mesh;
        MeshRenderer renderer = building.GetComponent<MeshRenderer>();
        renderer.material = material;
    }
}
