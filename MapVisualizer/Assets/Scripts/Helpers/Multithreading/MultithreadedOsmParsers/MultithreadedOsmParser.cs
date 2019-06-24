using System.Threading;

public abstract class MultithreadedOsmParser : MultithreadedEntity
{
    public Tile Tile { get; private set; }

    protected string response;

    public MultithreadedOsmParser(Tile tile, string response)
    {
        Tile = tile;
        this.response = response;
    }
}
