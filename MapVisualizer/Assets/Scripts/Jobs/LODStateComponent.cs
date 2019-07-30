using System;
using Unity.Entities;

[Serializable]
public struct LODState: IComponentData
{
    public bool IsClose;
}

public class LODStateComponent : ComponentDataProxy<LODState>
{
    
}
