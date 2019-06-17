using System;
using System.Collections.Generic;
using UnityEngine;

public class Road: Surface, IComparable<Road>
{
    public int Lanes { get; private set; }

    public enum RoadType { Default,
                           Motorway, Trunk, Primary, Secondary, Tertiary, Unclassified, Residential,
                           Living_street, Service, Pedestrian, Track, Bus_guideway, Escape, Raceway, Road,
                           Footway, Bridleway, Steps, Path }

    public RoadType Type { get; private set; }

    public Road(int lanes, Vector2[] nodes, RoadType type): base(nodes)
    {
        Lanes = lanes;
        Type = type;
    }

    public static RoadType GetRoadType(string roadType)
    {
        return EnumHelper.GetTypeFromString<RoadType>(roadType, RoadType.Default);
    }

    public float GetRoadWidth()
    {
        switch (Type)
        {
            case RoadType.Default:                
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Motorway:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Trunk:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Primary:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Secondary:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Tertiary:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Unclassified:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Residential:
                if (Lanes > 0)
                {
                    return NumericConstants.ROAD_LANE_WIDTH * Lanes;
                }
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Living_street:
                return 2 * NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Service:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Pedestrian:
                return 4 * NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Track:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Bus_guideway:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Escape:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Raceway:
                return 2 * NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Road:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Footway:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Bridleway:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Steps:
                return NumericConstants.ROAD_LANE_WIDTH;
            case RoadType.Path:
                return NumericConstants.ROAD_LANE_WIDTH;
        }

        return NumericConstants.ROAD_LANE_WIDTH;
    }

    public int CompareTo(Road other)
    {
        if (Type == RoadType.Default)
            return -1;

        if (other.Type == RoadType.Default)
            return 1;

        if (other.Type > Type)
            return 1;
        else if (other.Type < Type)
            return -1;

        return 0;
    }
}
