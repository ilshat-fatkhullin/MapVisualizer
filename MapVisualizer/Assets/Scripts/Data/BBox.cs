public class BBox
{
    public float MinLongitude { get; private set; }

    public float MinLatitude { get; private set; }

    public float MaxLongitude { get; private set; }

    public float MaxLatitude { get; private set; }

    public BBox(float minLongitude, float minLatitude, float maxLongitude, float maxLatitude)
    {
        MinLongitude = minLongitude;
        MinLatitude = minLatitude;
        MaxLongitude = maxLongitude;
        MaxLatitude = maxLatitude;
    }
}
