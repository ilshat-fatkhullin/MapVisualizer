using System.Collections.Generic;
using UnityEngine;

public abstract class Surface
{
    public List<Vector2> Nodes { get; private set; }

    public Surface(List<Vector2> nodes)
    {
        Nodes = nodes;
    }
}
