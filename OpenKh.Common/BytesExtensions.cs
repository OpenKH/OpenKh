namespace OpenKh.Common
{
    public static class BytesExtensions
    {
        public static byte ToByte(this byte[] data, int offset) =>
            data[offset];

        public static short ToShort(this byte[] data, int offset) =>
            (short)(data[offset] | (data[offset + 1] << 8));

        public static int ToInt(this byte[] data, int offset) =>
            data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
    }
}
