using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    private void OnBecameVisible()
    {
        CombineMeshes(gameObject);
        Destroy(this);
    }

    private void CombineMeshes(GameObject container)
    {
        MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        MeshFilter meshFilter = container.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.Optimize();
        container.gameObject.SetActive(true);
    }
}
