public abstract class ObjectToInstantiate
{
    public Tile Tile { get; private set; }

    public ObjectToInstantiate(Tile tile)
    {
        Tile = tile;
    }
}
