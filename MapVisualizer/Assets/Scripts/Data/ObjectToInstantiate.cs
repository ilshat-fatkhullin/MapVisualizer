using UnityEngine;

public class ObjectToInstantiate
{
    public MeshInfo Info { get; private set; }

    public Material Material { get; private set; }

    public Tile Tile { get; private set; }

    public ObjectToInstantiate(MeshInfo info, Material material, Tile tile)
    {
        Info = info;
        Material = material;
        Tile = tile;
    }
}
