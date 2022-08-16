namespace Aurora.Shared.Extensions;

public static class EnumHelper
{
    public static Dictionary<int, string> EnumValueToName<T>() where T : struct, Enum
    {
        T[] vals = (T[])Enum.GetValues(typeof(T));
        return vals.ToDictionary(x => Int32.Parse(x.ToString("D")), y => y.ToString());
    }
}
