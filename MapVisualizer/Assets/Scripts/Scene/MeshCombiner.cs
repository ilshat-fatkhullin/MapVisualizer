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
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.Optimize();
        GetComponent<MeshRenderer>().materials = meshFilters[meshFilters.Length - 1].GetComponent<MeshRenderer>().materials;
        gameObject.SetActive(true);
    }
}
