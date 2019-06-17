using System.Collections.Generic;

public class OsmWay
{
    private List<OsmNode> nodes;

    private Dictionary<string, string> tagKeyToValue;

    public OsmWay()
    {
        nodes = new List<OsmNode>();
        tagKeyToValue = new Dictionary<string, string>();
    }

    public string GetTagValue(string key)
    {
        if (tagKeyToValue.TryGetValue(key, out string result))
        {
            return result;
        }

        return null;
    }

    public void AddTagValue(string key, string value)
    {
        tagKeyToValue.Add(key, value);
    }

    public List<OsmNode> GetNodes()
    {
        return nodes;
    }

    public void AddNode(OsmNode node)
    {
        nodes.Add(node);
    }
}
