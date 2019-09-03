using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    List<Mesh> meshes = new List<Mesh>();
    List<MeshFilter> meshFilters = new List<MeshFilter>();

    public void AddMesh(Mesh mesh, MeshFilter meshFilter)
    {
        meshes.Add(mesh);
        meshFilters.Add(meshFilter);
    }

    public void CombineMeshes()
    {
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        
        for (int i = 0; i < meshFilters.Count; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            meshFilters[i].transform.position -= transform.position;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;            
            meshFilters[i].gameObject.SetActive(false);
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.Optimize();
        GetComponent<MeshRenderer>().materials = meshFilters[meshFilters.Count - 1].GetComponent<MeshRenderer>().materials;

        for (int i = 0; i < meshFilters.Count; i++)
        {            
            Destroy(meshFilters[i].gameObject);
        }        
    }
}
