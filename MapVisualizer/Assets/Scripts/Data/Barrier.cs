using UnityEngine;

public class Barrier
{
    public Vector2[] Points { get; private set; }

    public enum BarrierType { Kerb, Fence }

    public BarrierType Type { get; private set; }

    public Barrier(Vector2[] points, BarrierType type)
    {
        Points = points;
        Type = type;
    }

    public static BarrierType GetBarrierType(string barrierType)
    {
        return EnumHelper.GetTypeFromString(barrierType, BarrierType.Kerb);
    }
}
