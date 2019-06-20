using System;
using System.Collections.Generic;
using UnityEngine;

public static class OsmFileParser
{
    public static List<Road> GetRoads(OsmFile file)
    {
        List<Road> roads = new List<Road>();

        foreach (OsmWay way in file.GetWays())
        {
            List<OsmNode> nodes = way.GetNodes();
            Vector2[] points = new Vector2[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                points[i] = nodes[i].Coordinate;
            }

            int lanes = 1;
            float width = 0;

            Road.RoadType roadType = Road.RoadType.Default;

            string lanesStr = way.GetTagValue("lanes");
            string widthStr = way.GetTagValue("width");
            string highwaysStr = way.GetTagValue("highway");

            if (lanesStr != null)
            {
                lanes = Convert.ToInt32(lanesStr);
            }

            if (widthStr != null)
            {
                width = Convert.ToSingle(widthStr, CultureInfoHelper.EnUSInfo);
            }

            if (highwaysStr != null)
            {
                roadType = Road.GetRoadType(highwaysStr);
            }

            if (roadType != Road.RoadType.Default)
            {
                roads.Add(new Road(lanes, width, points, roadType));
            }
        }

        return roads;
    }

    public static List<Area> GetAreas(OsmFile file)
    {
        List<Area> areas = new List<Area>();

        foreach (OsmWay way in file.GetWays())
        {
            List<OsmNode> nodes = way.GetNodes();
            Vector2[] points = new Vector2[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                points[i] = nodes[i].Coordinate;
            }

            Area.AreaType areaType = Area.AreaType.Default;

            string landuseStr = way.GetTagValue("landuse");
            string leisureStr = way.GetTagValue("leisure");

            if (landuseStr != null)
            {
                areaType = Area.GetAreaType(landuseStr);
            }

            if (leisureStr != null)
            {
                areaType = Area.GetAreaType(leisureStr);
            }

            if (areaType != Area.AreaType.Default)
            {
                areas.Add(new Area(points, areaType));
            }
        }

        return areas;
    }
}
