using OpenKh.Command.AnbMaker.Utils.Builder.Models;

namespace OpenKh.Command.AnbMaker.Extensions
{
    internal static class BuilderExtensions
    {
        public static float GetInterpolatedVector(this AScalarKey[] keys, double time)
        {
            if (keys.Any())
            {
                if (time < keys.First().Time)
                {
                    return keys.First().Value;
                }
                if (keys.Last().Time < time)
                {
                    return keys.Last().Value;
                }

                for (int x = 0, cx = keys.Length - 1; x < cx; x++)
                {
                    var time0 = keys[x].Time;
                    var time1 = keys[x + 1].Time;

                    if (time0 <= time && time <= time1)
                    {
                        float ratio = (float)((time - time0) / (time1 - time0));

                        return (keys[x].Value * (1f - ratio)) + (keys[x + 1].Value * ratio);
                    }
                }
            }

            return 0;
        }

    }
}
