using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class RoadsVisualizer : Visualizer
{
    public Terrain Terrain;

    private CultureInfo info = new CultureInfo("en-US");

    private Dictionary<string, Vector2> allRoadNodes;

    private List<Road> roads;

    private Intmap intmap;

    private Vector2 terrainSize;

    private void Awake()
    {
        roads = new List<Road>();
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
        UpdateTerrainTransform();
        RetrieveRoads(response);
        UpdateTerrain();
    }

    private void RetrieveRoads(string response)
    {
        allRoadNodes = new Dictionary<string, Vector2>();
        roads = new List<Road>();

        XmlDocument document = new XmlDocument();
        document.LoadXml(response);
        XmlElement root = document.DocumentElement;

        Vector2 terrainOrigin = new Vector2(Terrain.transform.position.x, Terrain.transform.position.z);

        foreach (XmlNode node in root)
        {
            if (node.Name == "node")
            {

                Vector2 coordinates = GeoPositioningHelper.GetMetersFromCoordinate(
                    new Coordinate(
                    Convert.ToSingle(node.Attributes.GetNamedItem("lat").Value, info),
                    Convert.ToSingle(node.Attributes.GetNamedItem("lon").Value, info)
                    )) - originInMeters - terrainOrigin;

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
                        }
                    }
                }

                roads.Add(new Road(lanes, roadNodes, roadType));
            }
        }
    }

    private void UpdateTerrainTransform()
    {
        Terrain.Flush();

        Vector2 nw = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(currentTile));
        Vector2 se = GeoPositioningHelper.GetMetersFromCoordinate(GeoPositioningHelper.GetNWCoordinateFromTile(new Tile(currentTile.X + 1, currentTile.Y + 1, currentTile.Zoom)));
        terrainSize = new Vector2(Math.Abs(nw.x - se.x), Math.Abs(nw.y - se.y));

        Terrain.terrainData.size = new Vector3(terrainSize.x, Terrain.terrainData.size.y, terrainSize.y);

        Terrain.gameObject.transform.position = new Vector3(nw.x - originInMeters.x, 0, se.y - originInMeters.y);
    }

    private void UpdateTerrain()
    {        
        intmap = new Intmap(Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight);

        foreach (var road in roads)
        {
            if (road.Nodes.Count < 2)
                continue;

            for (int i = 1; i < road.Nodes.Count; i++)
            {
                Point2D point1 = GetTerrainMapPoint(road.Nodes[i - 1], terrainSize);
                Point2D point2 = GetTerrainMapPoint(road.Nodes[i], terrainSize);
                intmap.DrawLine(point1, point2, (int)road.Type,
                                MetersToTerrainMapCells(road.GetRoadWidth(), terrainSize.x));
            }
        }

        float[,,] alphamap = new float[Terrain.terrainData.alphamapWidth, Terrain.terrainData.alphamapHeight, Terrain.terrainData.alphamapLayers];

        for (int x = 0; x < alphamap.GetLength(0); x++)
            for (int y = 0; y < alphamap.GetLength(1); y++)
                for (int l = 0; l < alphamap.GetLength(2); l++)
                {
                    if (intmap.Map[x, y] == l)
                    {
                        alphamap[y, x, l] = 1;
                    }
                    else
                    {
                        alphamap[y, x, l] = 0;
                    }
                }

        Terrain.terrainData.SetAlphamaps(0, 0, alphamap);
    }

    private Point2D GetTerrainMapPoint(Vector2 coordinate, Vector2 terrainSize)
    {
        return new Point2D(Mathf.RoundToInt((coordinate.x / terrainSize.x) * Terrain.terrainData.alphamapWidth),
                           Mathf.RoundToInt((coordinate.y / terrainSize.y) * Terrain.terrainData.alphamapHeight));
    }

    private int MetersToTerrainMapCells(float meters, float terrainSize)
    {
        return Mathf.RoundToInt(meters * Terrain.terrainData.alphamapWidth / terrainSize);
    }
}
