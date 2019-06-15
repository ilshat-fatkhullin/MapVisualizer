using EasyRoads3Dv3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class RoadsVisualizer : Visualizer<RoadToInstantiate>
{
    public Material AsphaultMaterial;

    private Dictionary<string, Vector2> allRoadNodes;

    private CultureInfo info = new CultureInfo("en-US");

    private ERRoadNetwork roadNetwork;

    private ERRoadType[] roadTypes;

    private void Awake()
    {
        roadNetwork = new ERRoadNetwork();

        string[] names = Enum.GetNames(typeof(Road.RoadType));
        roadTypes = new ERRoadType[names.Length];
        for (int i = 0; i < roadTypes.Length; i++)
        {
            roadTypes[i] = roadNetwork.GetRoadTypeByName(names[i]);
        }
    }

    protected override void Start()
    {
        base.Start();
    }


    protected override string BuildRequest(Tile tile)
    {
        BBox bBox = GeoPositioningHelper.GetBBoxFromTile(tile);
        return string.Format(StringConstants.OSMTileRequestPattern,
            Convert.ToString(bBox.MinLongitude, info), Convert.ToString(bBox.MinLatitude, info),
            Convert.ToString(bBox.MaxLongitude, info), Convert.ToString(bBox.MaxLatitude, info));
    }

    protected override void OnNetworkResponse(Tile tile, string response)
    {
        if (!map.ContainsTile(tile))
        {
            return;
        }

        if (string.IsNullOrEmpty(response))
        {
            return;
        }

        allRoadNodes = new Dictionary<string, Vector2>();

        XmlDocument document = new XmlDocument();
        document.LoadXml(response);
        XmlElement root = document.DocumentElement;

        foreach (XmlNode node in root)
        {
            if (node.Name == "node")
            {                

                Vector2 coordinates = GeoPositioningHelper.GetMetersFromCoordinate(
                    new Coordinate(
                    Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value, info),
                    Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value, info)
                    )) - originInMeters;

                allRoadNodes.Add(node.Attributes.GetNamedItem("id").Value, coordinates);
            }
        }

        foreach (XmlNode node in root)
        {
            if (node.Name == "way")
            {
                List<Vector2> roadNodes = new List<Vector2>();

                int lanes = 1;
                Road.RoadType roadType = Road.RoadType.Default;
                string roadName = "Default";

                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == "nd")
                    {
                        roadNodes.Add(allRoadNodes[subNode.Attributes.GetNamedItem("ref").Value]);
                    }
                    else if (subNode.Name == "tag")
                    {
                        string key = subNode.Attributes.GetNamedItem("k").Value;
                        string value = subNode.Attributes.GetNamedItem("v").Value;

                        if (key == "lanes")
                        {
                            lanes = Convert.ToInt32(value);
                        }
                        else if (key == "highway")
                        {
                            value = char.ToUpper(value[0]) + value.Substring(1);
                            roadType = Road.GetRoadType(value);
                            roadName = value;
                        }
                    }
                }

                Road road = new Road(lanes, roadNodes, roadType, roadName);
                InstantiateRoad(road, tile);
            }
        }
    }

    private void InstantiateRoad(Road road, Tile tile)
    {        
        if (road.Type == Road.RoadType.Default)
            return;

        map.EnqueueObjectToInstantitate(new RoadToInstantiate(road, tile));
    }

    protected override GameObject InstantiateObject(RoadToInstantiate objectToInstantiate)
    {
        Road road = objectToInstantiate.Road;
        Vector3[] points = new Vector3[road.Nodes.Count];        

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(road.Nodes[i].x, NumericConstants.ROAD_Y_OFFSET, road.Nodes[i].y);
        }

        ERRoadType roadType = roadTypes[(int)objectToInstantiate.Road.Type];
        ERRoad erRoad = roadNetwork.CreateRoad(objectToInstantiate.Road.Name, roadType, points);

        return erRoad.gameObject;
    }
}
