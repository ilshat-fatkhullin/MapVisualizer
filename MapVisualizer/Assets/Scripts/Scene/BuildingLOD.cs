using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BuildingLOD : MonoBehaviour
{
    public MeshRenderer MeshRenderer;

    public float LODSwitchDistance;

    private float sqrLODSwitchDistance;

    private bool isClose;

    private void Awake()
    {
        sqrLODSwitchDistance = LODSwitchDistance * LODSwitchDistance;
    }

    private void Start()
    {
        isClose = true;
        SetLOD(false);
    }

    private void Update()
    {
        SetLOD(Vector3.SqrMagnitude(Camera.main.transform.position - transform.position) < sqrLODSwitchDistance);
    }

    private void SetLOD(bool isClose)
    {
        if (this.isClose == isClose)
            return;
        this.isClose = isClose;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isClose);
        }
        MeshRenderer.enabled = !isClose;
    }
}
