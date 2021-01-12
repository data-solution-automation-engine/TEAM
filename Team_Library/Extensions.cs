using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
