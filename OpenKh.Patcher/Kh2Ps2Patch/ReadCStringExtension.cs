using System.IO;
using System.Text;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    internal static class ReadCStringExtension
    {
        public static string ReadCString(this Stream stream)
        {
            var bytes = new MemoryStream();

            while (true)
            {
                var one = stream.ReadByte();
                if (one < 0 || one == 0)
                {
                    break;
                }

                bytes.WriteByte((byte)one);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
