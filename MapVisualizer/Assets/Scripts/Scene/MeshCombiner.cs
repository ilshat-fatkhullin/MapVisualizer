using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    private void OnBecameVisible()
    {
        CombineMeshes();
        Destroy(this);
    }

    private void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
        
        for (int i = 1; i < meshFilters.Length; i++)
        {
            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            meshFilters[i].transform.position -= transform.position;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;            
            meshFilters[i].gameObject.SetActive(false);
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.Optimize();
        GetComponent<MeshRenderer>().materials = meshFilters[meshFilters.Length - 1].GetComponent<MeshRenderer>().materials;

        for (int i = 1; i < meshFilters.Length; i++)
        {            
            Destroy(meshFilters[i].gameObject);
        }        
    }
}
