using System;

namespace OpenKh.Engine
{
    public static class MathEx
    {
        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static float CubicHermite(float t, float p0, float p1, float m0, float m1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * p1 + (t3 - t2) * m1;
        }

        /// <see cref="https://stackoverflow.com/a/1971667"/>
        public static float Modulus(float dividend, float divisor)
        {
            return (float)((Math.Abs(dividend) - (Math.Abs(divisor) *
                (Math.Floor(Math.Abs(dividend) / Math.Abs(divisor))))) *
                Math.Sign(dividend));
        }
    }
}
