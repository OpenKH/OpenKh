namespace OpenKh.Kh2
{
    public partial class Ps2
    {
        private static readonly byte[] tbl8bc =
        {
            0, 1, 4, 5, 16, 17, 20, 21, 2, 3, 6, 7, 18, 19, 22, 23,
            8, 9, 12, 13, 24, 25, 28, 29, 10, 11, 14, 15, 26, 27, 30, 31
        };

        private static readonly byte[] tbl8c0 =
        {
            0x00, 0x04, 0x10, 0x14, 0x20, 0x24, 0x30, 0x34, 0x02, 0x06, 0x12, 0x16, 0x22, 0x26, 0x32, 0x36,
            0x08, 0x0c, 0x18, 0x1c, 0x28, 0x2c, 0x38, 0x3c, 0x0a, 0x0e, 0x1a, 0x1e, 0x2a, 0x2e, 0x3a, 0x3e,
            0x21, 0x25, 0x31, 0x35, 0x01, 0x05, 0x11, 0x15, 0x23, 0x27, 0x33, 0x37, 0x03, 0x07, 0x13, 0x17,
            0x29, 0x2d, 0x39, 0x3d, 0x09, 0x0d, 0x19, 0x1d, 0x2b, 0x2f, 0x3b, 0x3f, 0x0b, 0x0f, 0x1b, 0x1f
        };

        private static readonly byte[] tbl8c1 =
        {
            0x20, 0x24, 0x30, 0x34, 0x00, 0x04, 0x10, 0x14, 0x22, 0x26, 0x32, 0x36, 0x02, 0x06, 0x12, 0x16,
            0x28, 0x2c, 0x38, 0x3c, 0x08, 0x0c, 0x18, 0x1c, 0x2a, 0x2e, 0x3a, 0x3e, 0x0a, 0x0e, 0x1a, 0x1e,
            0x01, 0x05, 0x11, 0x15, 0x21, 0x25, 0x31, 0x35, 0x03, 0x07, 0x13, 0x17, 0x23, 0x27, 0x33, 0x37,
            0x09, 0x0d, 0x19, 0x1d, 0x29, 0x2d, 0x39, 0x3d, 0x0b, 0x0f, 0x1b, 0x1f, 0x2b, 0x2f, 0x3b, 0x3f
        };

        public static byte[] Decode8(byte[] bin, int bw, int bh)
        {
            var buffer = new byte[bin.Length];
            for (int i = 0; i < (0x40 * bh); i += 0x40)
            {
                for (int j = 0; j < (0x80 * bw); j += 0x80)
                {
                    int num3 = 0x2000 * ((j / 0x80) + (bw * (i / 0x40)));
                    for (int k = 0; k < 0x40; k += 0x10)
                    {
                        for (int m = 0; m < 0x80; m += 0x10)
                        {
                            int num6 = 0x100 * tbl8bc[(m / 0x10) + (8 * (k / 0x10))];
                            for (int n = 0; n < 4; n++)
                            {
                                int num8 = 0x40 * n;
                                byte[] buffer2 = ((n & 1) == 0) ? tbl8c0 : tbl8c1;
                                for (int num9 = 0; num9 < 0x40; num9++)
                                {
                                    int index = ((num3 + num6) + num8) + buffer2[num9];
                                    int num11 = (j + m) + (num9 % 0x10);
                                    int num12 = ((i + k) + (4 * n)) + (num9 / 0x10);
                                    int num13 = num11 + ((0x80 * bw) * num12);
                                    buffer[num13] = bin[index];
                                }
                            }
                        }
                    }
                }
            }
            return buffer;
        }

        public static byte[] Encode8(byte[] bin, int bw, int bh)
        {
            var buffer = new byte[bin.Length];
            for (int i = 0; i < (0x40 * bh); i += 0x40)
            {
                for (int j = 0; j < (0x80 * bw); j += 0x80)
                {
                    int num3 = 0x2000 * ((j / 0x80) + (bw * (i / 0x40)));
                    for (int k = 0; k < 0x40; k += 0x10)
                    {
                        for (int m = 0; m < 0x80; m += 0x10)
                        {
                            int num6 = 0x100 * tbl8bc[(m / 0x10) + (8 * (k / 0x10))];
                            for (int n = 0; n < 4; n++)
                            {
                                int num8 = 0x40 * n;
                                byte[] buffer2 = ((n & 1) == 0) ? tbl8c0 : tbl8c1;
                                for (int num9 = 0; num9 < 0x40; num9++)
                                {
                                    int index = ((num3 + num6) + num8) + buffer2[num9];
                                    int num11 = (j + m) + (num9 % 0x10);
                                    int num12 = ((i + k) + (4 * n)) + (num9 / 0x10);
                                    int num13 = num11 + ((0x80 * bw) * num12);
                                    buffer[index] = bin[num13];
                                }
                            }
                        }
                    }
                }
            }
            return buffer;
        }
    }
}
