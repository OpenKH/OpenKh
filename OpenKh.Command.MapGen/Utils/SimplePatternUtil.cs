using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenKh.Command.MapGen.Utils
{
    public static class SimplePatternUtil
    {
        internal static Regex CreateFrom(string name) =>
            new Regex(
                ("^" + Regex.Escape(name) + "$")
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".?"),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
            );
    }
}
