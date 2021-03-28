namespace OpenKh.Kh2
{
    public partial class Ps2
    {
        private static readonly byte[] tbl32bc =
        {
            0, 1, 4, 5, 16, 17, 20, 21, 2, 3, 6, 7, 18, 19, 22, 23,
            8, 9, 12, 13, 24, 25, 28, 29, 10, 11, 14, 15, 26, 27, 30, 31
        };

        private static readonly byte[] tbl32pao = { 0, 1, 4, 5, 8, 9, 12, 13, 2, 3, 6, 7, 10, 11, 14, 15 };

        public static byte[] Decode32(byte[] bin, int bw, int bh)
        {
            var buffer = new byte[bin.Length];
            for (int i = 0; i < (0x20 * bh); i += 0x20)
            {
                for (int j = 0; j < (0x40 * bw); j += 0x40)
                {
                    int num3 = 0x2000 * ((j / 0x40) + (bw * (i / 0x20)));
                    for (int k = 0; k < 0x20; k += 8)
                    {
                        for (int m = 0; m < 0x40; m += 8)
                        {
                            int num6 = 0x100 * tbl32bc[(m / 8) + ((k / 8) * 8)];
                            for (int n = 0; n < 4; n++)
                            {
                                int num8 = 0x40 * n;
                                for (int num9 = 0; num9 < 0x10; num9++)
                                {
                                    int num10 = (j + m) + (num9 % 8);
                                    int num11 = ((i + k) + (2 * n)) + (num9 / 8);
                                    int index = 4 * (num10 + ((0x40 * bw) * num11));
                                    int num13 = (((4 * tbl32pao[num9]) + num8) + num6) + num3;
                                    buffer[index] = bin[num13];
                                    buffer[index + 1] = bin[num13 + 1];
                                    buffer[index + 2] = bin[num13 + 2];
                                    buffer[index + 3] = bin[num13 + 3];
                                }
                            }
                        }
                    }
                }
            }
            return buffer;
        }

        public static byte[] Encode32(byte[] bin, int bw, int bh)
        {
            var buffer = new byte[bin.Length];
            for (int i = 0; i < (0x20 * bh); i += 0x20)
            {
                for (int j = 0; j < (0x40 * bw); j += 0x40)
                {
                    int num3 = 0x2000 * ((j / 0x40) + (bw * (i / 0x20)));
                    for (int k = 0; k < 0x20; k += 8)
                    {
                        for (int m = 0; m < 0x40; m += 8)
                        {
                            int num6 = 0x100 * tbl32bc[(m / 8) + ((k / 8) * 8)];
                            for (int n = 0; n < 4; n++)
                            {
                                int num8 = 0x40 * n;
                                for (int num9 = 0; num9 < 0x10; num9++)
                                {
                                    int num10 = (j + m) + (num9 % 8);
                                    int num11 = ((i + k) + (2 * n)) + (num9 / 8);
                                    int index = 4 * (num10 + ((0x40 * bw) * num11));
                                    int num13 = (((4 * tbl32pao[num9]) + num8) + num6) + num3;
                                    buffer[num13] = bin[index];
                                    buffer[num13 + 1] = bin[index + 1];
                                    buffer[num13 + 2] = bin[index + 2];
                                    buffer[num13 + 3] = bin[index + 3];
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
