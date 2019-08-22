namespace OpenKh.Imaging
{
    public static class ImageDataHelpers
    {
        public static byte[] FromIndexed8ToBgra(byte[] data, byte[] clut)
        {
            var bitmap = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                var clutIndex = data[i];
                bitmap[i * 4 + 0] = clut[clutIndex * 4 + 2];
                bitmap[i * 4 + 1] = clut[clutIndex * 4 + 1];
                bitmap[i * 4 + 2] = clut[clutIndex * 4 + 0];
                bitmap[i * 4 + 3] = clut[clutIndex * 4 + 3];
            }
            return bitmap;
        }

        public static byte[] FromIndexed4ToBgra(byte[] data, byte[] clut)
        {
            var bitmap = new byte[data.Length * 8];
            for (int i = 0; i < data.Length; i++)
            {
                var subData = data[i];
                var clutIndex1 = subData >> 4;
                var clutIndex2 = subData & 0x0F;
                bitmap[i * 8 + 0] = clut[clutIndex1 * 4 + 2];
                bitmap[i * 8 + 1] = clut[clutIndex1 * 4 + 1];
                bitmap[i * 8 + 2] = clut[clutIndex1 * 4 + 0];
                bitmap[i * 8 + 3] = clut[clutIndex1 * 4 + 3];
                bitmap[i * 8 + 4] = clut[clutIndex2 * 4 + 2];
                bitmap[i * 8 + 5] = clut[clutIndex2 * 4 + 1];
                bitmap[i * 8 + 6] = clut[clutIndex2 * 4 + 0];
                bitmap[i * 8 + 7] = clut[clutIndex2 * 4 + 3];
            }
            return bitmap;
        }
    }
}
