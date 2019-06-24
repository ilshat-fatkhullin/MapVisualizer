using System.Collections.Generic;
using UnityEngine;

public class MarkupVisualizer : Singleton<MarkupVisualizer>
{
    public GameObject DividingStripContainerPrefab;

    public GameObject DividingStripPrefab;

    [Range(1f, 10f)]
    public float Step;

    private List<GameObject> stripContainers;

    private void Awake()
    {
        stripContainers = new List<GameObject>();
    }

    public void VisualizeTile(Tile tile, List<Road> roads, Vector2 originInMeters)
    {
        foreach (var strip in stripContainers)
        {
            Destroy(strip);
        }

        foreach (var road in roads)
        {
            Road normalizedRoad = new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(originInMeters), road.Type);

            if (road.Type == Road.RoadType.Primary || road.Type == Road.RoadType.Secondary || road.Type == Road.RoadType.Tertiary)
            {
                InstantiateMarkup(normalizedRoad);
            }
        }     
    }

    private void InstantiateMarkup(Road road)
    {
        for (int i = 1; i < road.Nodes.Length; i++)
        {            
            Vector2 point1 = road.Nodes[i - 1];
            Vector2 point2 = road.Nodes[i];

            Vector2 direction2D = point2 - point1;            

            float length = direction2D.magnitude;            

            direction2D.Normalize();
            Vector3 direction3D = new Vector3(direction2D.x, 0, direction2D.y);
            Vector2 point = point1;

            while (length >= Step)
            {
                point += direction2D * Step;
                length -= Step;

                InstantiateStripes(road.Lanes, road.GetRoadWidth(),
                    new Vector3(point.x, NumericConstants.ROAD_Y_OFFSET, point.y),
                    direction3D);
            }
        }
    }

    private void InstantiateStripes(int lanes, float width, Vector3 point, Vector3 direction)
    {
        Vector3 leftDirection = new Vector3(direction.z, direction.y, -direction.x);

        Vector3 leftPoint = point + leftDirection * (width / 2f);
        Vector3 rightPoint = point - leftDirection * (width / 2f);

        Quaternion rotation =  Quaternion.LookRotation(direction);

        GameObject stripsContainer = Instantiate(DividingStripContainerPrefab);

        for (int i = 1; i < lanes; i++)
        {
            GameObject strip = Instantiate(DividingStripPrefab,
                       Vector3.Lerp(leftPoint, rightPoint, (float)i / lanes),
                       rotation);

            strip.transform.parent = stripsContainer.transform;         
        }

        stripContainers.Add(stripsContainer);
    }
}
