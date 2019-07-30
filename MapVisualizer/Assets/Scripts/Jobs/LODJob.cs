using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct LODJob : IJobParallelFor
{
    [ReadOnly]
    public float3 CameraPosition;

    [ReadOnly]
    public float SqrLODSwitchDistance;

    [ReadOnly]
    public NativeArray<float3> BuildingPositions;

    public NativeArray<bool> Results;

    public void Execute(int index)
    {
        float distance = math.distancesq(CameraPosition, BuildingPositions[index]);
        Results[index] = distance < SqrLODSwitchDistance;
    }
}
