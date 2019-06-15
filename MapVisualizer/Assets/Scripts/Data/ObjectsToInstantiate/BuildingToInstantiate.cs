using UnityEngine;

public class BuildingToInstantiate: ObjectToInstantiate
{
    public MeshInfo Info { get; private set; }

    public Material Material { get; private set; }

    public BuildingToInstantiate(MeshInfo info, Material material, Tile tile): base(tile)
    {
        Info = info;
        Material = material;
    }
}
