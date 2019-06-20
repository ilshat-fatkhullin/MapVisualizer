using UnityEngine;

public abstract class Surface
{
    public Vector2[] Nodes { get; private set; }

    public Surface(Vector2[] nodes)
    {
        Nodes = nodes;
    }

    public Vector2[] GetNodesRelatedToOrigin(Vector2 origin)
    {
        Vector2[] nodes = new Vector2[Nodes.Length];

        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i] = Nodes[i] - origin;
        }

        return nodes;
    }
}
