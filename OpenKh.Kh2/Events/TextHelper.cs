using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.Events
{
    internal class TextHelper
    {
        private static readonly Encoding _enc = Encoding.GetEncoding("latin1");

        internal static byte[] GetBytes(string name, int max)
        {
            var buffer = new byte[max];
            var bytes = _enc.GetBytes(name);
            Buffer.BlockCopy(bytes, 0, buffer, 0, Math.Min(bytes.Length, max));
            return buffer;
        }

        internal static string GetString(byte[] name) => _enc.GetString(name).Split('\0')[0];
    }
}
