using System;
using System.Collections.Generic;
using UnityEngine;

public class Area: Surface, IComparable<Area>
{
    public enum AreaType { Default, Grass, Park, Garden }

    public AreaType Type { get; private set; }

    public Area(Vector2[] nodes, AreaType type): base(nodes)
    {
        Type = type;
    }

    public static AreaType GetAreaType(string roadType)
    {
        return EnumHelper.GetTypeFromString<AreaType>(roadType, AreaType.Default);
    }

    public int CompareTo(Area other)
    {
        if (Type == AreaType.Default)
            return -1;

        if (other.Type == AreaType.Default)
            return 1;

        if (other.Type > Type)
            return 1;
        else if (other.Type < Type)
            return -1;

        return 0;
    }
}
