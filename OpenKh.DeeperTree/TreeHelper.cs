using System;

namespace OpenKh.DeeperTree
{
    internal static class TreeHelper
    {
        internal static bool IsPrimitiveValue(Type elementType)
        {
            return elementType == typeof(string)
                || elementType == typeof(int)
                || elementType == typeof(uint)
                || elementType == typeof(float)
                || elementType == typeof(double)
                || elementType == typeof(short)
                || elementType == typeof(ushort)
                || elementType == typeof(byte)
                || elementType == typeof(sbyte)
                || elementType.IsEnum
                ;
        }
    }
}
