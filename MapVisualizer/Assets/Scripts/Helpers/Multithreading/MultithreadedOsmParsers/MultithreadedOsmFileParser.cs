using System.Collections.Generic;

public class MultithreadedOsmFileParser : MultithreadedOsmParser
{
    public OsmFile OsmFile { get; private set; }

    public List<Road> Roads { get; private set; }

    public List<Area> Areas { get; private set; }

    public MultithreadedOsmFileParser(Tile tile, string response): base(tile, response)
    {

    }

    protected override void ExecuteThread()
    {
        OsmFile = new OsmFile(response);
        Roads = OsmFileParser.GetRoads(OsmFile);
        Areas = OsmFileParser.GetAreas(OsmFile);
    }
}
