using System.Collections.Generic;
using UnityEngine;

public class Road
{
    public int Lanes { get; private set; }

    public List<Vector2> Nodes { get; private set; }

    public enum RoadType { Default,
                           Motorway, Trunk, Primary, Secondary, Tertiary, Unclassified, Residential,
                           Living_street, Service, Pedestrian, Track, Bus_guideway, Escape, Raceway, Road,
                           Footway, Bridleway, Steps, Path }

    public RoadType Type { get; private set; }

    public string Name { get; private set; }

    public Road(int lanes, List<Vector2> nodes, RoadType type, string name)
    {
        Lanes = lanes;
        Nodes = nodes;
        Type = type;
        Name = name;
    }

    public static RoadType GetRoadType(string roadType)
    {
        try
        {
            RoadType type = (RoadType)System.Enum.Parse(typeof(RoadType), roadType);
            return type;
        }
        catch
        {
            return RoadType.Default;
        }
    }
}
