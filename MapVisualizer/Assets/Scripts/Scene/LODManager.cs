using System.Collections.Generic;
using UnityEngine;

public class LODManager : MonoBehaviour
{
    public static LODManager Instance { get { return Singleton<LODManager>.Instance; } }

    private List<GameObject> gameObjects;

    private void Awake()
    {
        gameObjects = new List<GameObject>();
    }

    public void AddGameObject(GameObject g)
    {
        gameObjects.Add(g);
    }

    private void Update()
    {
        
    }
}
