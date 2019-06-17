public static class EnumHelper
{
    public static T GetTypeFromString<T>(string type, T defaultValue)
    {
        try
        {
            T t = (T)System.Enum.Parse(typeof(T), type);
            return t;
        }
        catch
        {
            return defaultValue;
        }
    }
}
