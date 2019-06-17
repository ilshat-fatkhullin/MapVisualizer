using System.Collections.Generic;
using UnityEngine;

public abstract class Surface
{
    public Vector2[] Nodes { get; private set; }

    public Surface(Vector2[] nodes)
    {
        Nodes = nodes;
    }
}
