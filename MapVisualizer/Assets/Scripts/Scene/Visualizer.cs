using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Visualizer : MonoBehaviour
{    
    public float OriginLatitude;

    public float OriginLongitude;

    public int Zoom;

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
}
