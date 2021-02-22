using System.Linq;

namespace TEAM_Library
{
    public static class Extension
    {
        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }
    }
}
