using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class OsmFile
{
    private Dictionary<string, OsmNode> keyToNode;

    private Dictionary<string, OsmWay> keyToWay;

    public OsmFile(string file, Vector2 origin)
    {
        keyToNode = new Dictionary<string, OsmNode>();
        keyToWay = new Dictionary<string, OsmWay>();

        XmlDocument document = new XmlDocument();
        document.LoadXml(file);
        XmlElement root = document.DocumentElement;

        foreach (XmlNode node in root)
        {
            if (node.Name == "node")
            {

                Vector2 coordinates = GeoPositioningHelper.GetMetersFromCoordinate(
                    new Coordinate(
                    Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value, CultureInfoHelper.EnUSInfo),
                    Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value, CultureInfoHelper.EnUSInfo)
                    )) - origin;

                keyToNode.Add(node.Attributes.GetNamedItem("id").Value, new OsmNode(coordinates));
            }
        }

        foreach (XmlNode node in root)
        {
            if (node.Name == "way")
            {
                OsmWay way = new OsmWay();

                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == "nd")
                    {
                        string key = subNode.Attributes.GetNamedItem("ref").Value;
                        way.AddNode(keyToNode[key]);
                        continue;
                    }

                    if (subNode.Name == "tag")
                    {
                        string key = subNode.Attributes.GetNamedItem("k").Value;
                        string value = subNode.Attributes.GetNamedItem("v").Value;
                        value = char.ToUpper(value[0]) + value.Substring(1);
                        way.AddTagValue(key, value);
                    }
                }

                keyToWay.Add(node.Attributes.GetNamedItem("id").Value, way);
            }
        }
    }

    public OsmNode GetNode(string key)
    {
        if (keyToNode.TryGetValue(key, out OsmNode result))
        {
            return result;
        }

        return null;
    }

    public OsmNode[] GetNodes()
    {
        return keyToNode.Values.ToArray();
    }

    public OsmWay GetWay(string key)
    {
        if (keyToWay.TryGetValue(key, out OsmWay result))
        {
            return result;
        }

        return null;
    }

    public OsmWay[] GetWays()
    {
        return keyToWay.Values.ToArray();
    }
}
