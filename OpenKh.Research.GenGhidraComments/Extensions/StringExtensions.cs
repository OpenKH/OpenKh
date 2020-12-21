using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Research.GenGhidraComments.Extensions
{
    static class StringExtensions
    {
        internal static string CutBy(this string self, int maxLen)
        {
            return (self.Length > maxLen) ? self.Substring(0, maxLen - 3) + "..." : self;
        }
    }
}
