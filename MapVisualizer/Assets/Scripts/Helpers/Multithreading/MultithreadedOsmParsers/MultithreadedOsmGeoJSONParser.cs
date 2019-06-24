using BAMCIS.GeoJSON;
using Newtonsoft.Json;

public class MultithreadedOsmGeoJSONParser : MultithreadedOsmParser
{
    public FeatureCollection FeatureCollection;

    public MultithreadedOsmGeoJSONParser(Tile tile, string response) : base(tile, response)
    {

    }

    protected override void ExecuteThread()
    {
        FeatureCollection = JsonConvert.DeserializeObject<FeatureCollection>(response);
    }
}
