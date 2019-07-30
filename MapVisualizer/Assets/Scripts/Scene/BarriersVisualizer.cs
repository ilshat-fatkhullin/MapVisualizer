using System.Collections.Generic;
using UnityEngine;

public class BarriersVisualizer : Singleton<BarriersVisualizer>
{
    public GameObject BarrierContainerPrefab;

    public GameObject KerbPrefab;

    public GameObject FencePrefab;

    private GameObjectTilemap gameObjectTilemap;

    private void Start()
    {
        gameObjectTilemap = new GameObjectTilemap();
        VisualizingManager.Instance.OnOsmFileParsed.AddListener(VisualizeTile);
        VisualizingManager.Instance.OnTileRemoved.AddListener(gameObjectTilemap.RemoveTile);
    }

    public void VisualizeTile(MultithreadedOsmFileParser parser)
    {
        foreach (var way in parser.OsmFile.GetWays())
        {
            string barrierType = way.GetTagValue("barrier");

            if (barrierType == null)
                continue;

            List<OsmNode> nodes = way.GetNodes();
            Vector2[] points = new Vector2[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                points[i] = nodes[i].Coordinate - VisualizingManager.Instance.OriginInMeters;
            }

            Barrier barrier = new Barrier(points, Barrier.GetBarrierType(barrierType));

            InstantiateBarrier(parser.Tile, barrier);
        }
    }

    private void InstantiateBarrier(Tile tile, Barrier barrier)
    {
        GameObject container = Instantiate(BarrierContainerPrefab);

        GameObject prefab = GetPrefabByBarrierType(barrier.Type);

        for (int i = 1; i < barrier.Points.Length; i++)
        {
            Vector3 point1 = new Vector3(barrier.Points[i - 1].x, prefab.transform.lossyScale.y / 2, barrier.Points[i - 1].y);
            Vector3 point2 = new Vector3(barrier.Points[i].x, prefab.transform.lossyScale.y / 2, barrier.Points[i].y);

            Vector3 dir = point2 - point1;
            float length = dir.magnitude;
            dir.Normalize();
            Quaternion rotation = Quaternion.LookRotation(Quaternion.Euler(0, 90, 0) * dir);

            Vector3 point = point1;

            while (length > 0)
            {
                float width = Mathf.Min(length, prefab.transform.localScale.x);

                GameObject b = Instantiate(prefab, point + dir * (width / 2), rotation);
                b.transform.localScale = new Vector3(width, b.transform.localScale.y, b.transform.localScale.z);
                b.transform.parent = container.transform;

                point += dir * width;
                length -= width;
            }
        }

        gameObjectTilemap.AttachObjectToTile(tile, container);
    }

    private GameObject GetPrefabByBarrierType(Barrier.BarrierType barrierType)
    {
        switch (barrierType)
        {
            case Barrier.BarrierType.Kerb:
                return KerbPrefab;
            case Barrier.BarrierType.Fence:
                return FencePrefab;
            default:
                return KerbPrefab;
        }
    }
}
