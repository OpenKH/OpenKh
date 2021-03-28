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
    }
}
