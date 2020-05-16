using System;
using System.Collections.Generic;
using System.Text;

namespace vwBinTex2 {
    class KHcv4pal {
        readonly static sbyte[] tbl = new sbyte[] {
            /**/ 0, 1, 4, 5, 8, 9,12,13,
            /**/ 2, 3, 6, 7,10,11,14,15,
        };
        public static int repl(int t) {
            return tbl[t];
        }
    }
    class KHcv8pal {
        readonly static sbyte[] tbl = new sbyte[] {
            0,
            0,
            6,
            6,
            -2,
            -2,
            4,
            4,
            -4,
            -4,
            2,
            2,
            -6,
            -6,
            0,
            0,
            16,
            16,
            22,
            22,
            14,
            14,
            20,
            20,
            12,
            12,
            18,
            18,
            10,
            10,
            16,
            16,
            32,
            32,
            38,
            38,
            30,
            30,
            36,
            36,
            28,
            28,
            34,
            34,
            26,
            26,
            32,
            32,
            48,
            48,
            54,
            54,
            46,
            46,
            52,
            52,
            44,
            44,
            50,
            50,
            42,
            42,
            48,
            48,
            -48,
            -48,
            -42,
            -42,
            -50,
            -50,
            -44,
            -44,
            -52,
            -52,
            -46,
            -46,
            -54,
            -54,
            -48,
            -48,
            -32,
            -32,
            -26,
            -26,
            -34,
            -34,
            -28,
            -28,
            -36,
            -36,
            -30,
            -30,
            -38,
            -38,
            -32,
            -32,
            -16,
            -16,
            -10,
            -10,
            -18,
            -18,
            -12,
            -12,
            -20,
            -20,
            -14,
            -14,
            -22,
            -22,
            -16,
            -16,
            0,
            0,
            6,
            6,
            -2,
            -2,
            4,
            4,
            -4,
            -4,
            2,
            2,
            -6,
            -6,
            0,
            0,
        };
        public static int repl(int t) {
            return (t & 0x80) | ((t & 0x7F) + tbl[t & 0x7F]);
        }
    }
    class KHcv8pal_v2 {
        readonly static byte[] alt = new byte[] {
            /**/ 0, 1, 2, 3, 4, 5, 6, 7,16,17,18,19,20,21,22,23,
            /**/ 8, 9,10,11,12,13,14,15,24,25,26,27,28,29,30,31,
        };
        public static void repl(byte[] bSrc, int offSrc, byte[] bDst, int offDst) {
            for (int x = 0; x < 256; x++) {
                for (int t = 0; t < 4; t++)
                    bDst[offDst + 4 * (x) + t] = bSrc[offSrc + 4 * (alt[(x & 31)] + (x & (~31))) + t];
            }
        }
    }
    class KHcv4pal_v2 {
        readonly static sbyte[] tbl = new sbyte[] {
            /**/ 0, 1, 4, 5, 8, 9,12,13,
            /**/ 2, 3, 6, 7,10,11,14,15,
        };
        public static void repl(byte[] bSrc, int offSrc, byte[] bDst, int offDst) {
            for (int x = 0; x < 16; x++) {
                for (int t = 0; t < 4; t++)
                    bDst[offDst + 4 * (x) + t] = bSrc[offSrc + 4 * (tbl[x]) + t];
            }
        }
    }
    class KHcv8pal_swap34 {
        public static int repl(int x) {
            return 0
                | (((x & 0xE7)))
                | (((x & 0x10) != 0) ? 0x08 : 0x00)
                | (((x & 0x08) != 0) ? 0x10 : 0x00)
            ;
        }
    }
}
