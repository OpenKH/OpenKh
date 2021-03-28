namespace OpenKh.Common.Utils
{
    public static class BitsUtil
    {
        public class Int
        {
            public static bool GetBit(int Data, int position) => GetBits(Data, position, 1) != 0;

            public static int SetBit(int Data, int position, bool value) => SetBits(Data, position, 1, value ? 1 : 0);
            public static uint SetBit(uint Data, int position, bool value) => SetBits(Data, position, 1, (uint)(value ? 1 : 0));

            public static int GetBits(int Data, int position, int size)
            {
                var mask = (1 << size) - 1;
                return (Data >> position) & mask;
            }

            public static int SetBits(int Data, int position, int size, int value)
            {
                var mask = (int)((1 << size) - 1U);
                return (Data & ~(mask << position) | ((value & mask) << position));
            }

            public static uint SetBits(uint Data, int position, int size, uint value)
            {
                var mask = (int)((1 << size) - 1U);
                return (uint)(Data & ~(mask << position) | ((value & mask) << position));
            }

            public static int SignExtend(int value, int position, int bit)
            {
                value = value >> position;

                if ((value & (1 << bit)) != 0)
                {
                    return value - (1 << (bit + 1));
                }
                else
                {
                    return value;
                }
            }
        }

        public class Long
        {
            public static bool GetBit(long Data, int position) => GetBits(Data, position, 1) != 0;

            public static long SetBit(long Data, int position, bool value) => SetBits(Data, position, 1, value ? 1 : 0);

            public static long GetBits(long Data, int position, int size)
            {
                var mask = (1 << size) - 1;
                return (Data >> position) & mask;
            }

            public static long SetBits(long Data, int position, int size, int value)
            {
                var mask = (1 << size) - 1U;
                return Data & ~(mask << position) | ((value & mask) << position);
            }
        }
    }
}
