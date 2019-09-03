using System.Collections.Generic;
using UnityEngine;

public class MarkupVisualizer : Singleton<MarkupVisualizer>
{
    public GameObject DividingStripPrefab;

    public float Width;

    public float Length;

    [Range(1f, 10f)]
    public float Step;

    private GameObjectTilemap gameObjectTilemap;

    private void Start()
    {
        gameObjectTilemap = new GameObjectTilemap();
        VisualizingManager.Instance.OnOsmFileParsed.AddListener(VisualizeTile);
        VisualizingManager.Instance.OnTileRemoved.AddListener(gameObjectTilemap.RemoveTile);
    }

    public void VisualizeTile(MultithreadedOsmFileParser parser)
    {
        foreach (var road in parser.Roads)
        {
            Road normalizedRoad = new Road(road.Lanes, road.Width,
                road.GetNodesRelatedToOrigin(VisualizingManager.Instance.OriginInMeters), road.Type);

            if (road.Type == Road.RoadType.Primary || road.Type == Road.RoadType.Secondary || road.Type == Road.RoadType.Tertiary)
            {
                InstantiateMarkup(parser.Tile, normalizedRoad);
            }
        }     
    }

    private void InstantiateMarkup(Tile tile, Road road)
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

            Mesh mesh = CreateMesh(road.Lanes, road.GetRoadWidth());

            while (length >= Step)
            {
                point += direction2D * Step;
                length -= Step;

                InstantiateStripes(tile, mesh, new Vector3(point.x, NumericConstants.ROAD_Y_OFFSET, point.y), direction3D);
            }
        }
    }

    private Mesh CreateMesh(int lanes, float width)
    {
        Vector3 leftPoint = Vector3.left * (width / 2f);
        Vector3 rightPoint = Vector3.right * (width / 2f);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 1; i < lanes; i++)
        {
            Vector3 position = Vector3.Lerp(leftPoint, rightPoint, (float)i / lanes);

            vertices.Add(position + Vector3.left * Width + Vector3.back * Length);
            vertices.Add(position + Vector3.left * Width + Vector3.forward * Length);
            vertices.Add(position + Vector3.right * Width + Vector3.back * Length);
            vertices.Add(position + Vector3.right * Width + Vector3.forward * Length);

            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 2);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, vertices);
        mesh.RecalculateNormals();
        return mesh;
    }

    private void InstantiateStripes(Tile tile, Mesh mesh, Vector3 point, Vector3 direction)
    {
        GameObject strip = Instantiate(DividingStripPrefab , point, Quaternion.LookRotation(direction));
        strip.GetComponent<MeshFilter>().mesh = mesh;
        gameObjectTilemap.AttachObjectToTile(tile, strip);
    }
}
