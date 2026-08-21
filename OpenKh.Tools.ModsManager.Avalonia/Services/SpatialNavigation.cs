using Avalonia;
using Avalonia.Input;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public static class SpatialNavigation
{
    private const double DirectionTolerance = 1;

    public static int FindNearest(
        Rect source,
        IReadOnlyList<Rect> candidates,
        NavigationDirection direction)
    {
        var sourceCenter = GetCenter(source);
        var bestIndex = -1;
        var bestScore = double.MaxValue;

        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            if (candidate.Width <= 0 || candidate.Height <= 0)
                continue;

            var candidateCenter = GetCenter(candidate);
            if (!IsInDirection(source, candidate, direction))
                continue;

            var primaryDistance = direction is NavigationDirection.Left or NavigationDirection.Right
                ? Math.Abs(candidateCenter.X - sourceCenter.X)
                : Math.Abs(candidateCenter.Y - sourceCenter.Y);
            var crossDistance = direction is NavigationDirection.Left or NavigationDirection.Right
                ? Math.Abs(candidateCenter.Y - sourceCenter.Y)
                : Math.Abs(candidateCenter.X - sourceCenter.X);
            var crossGap = direction is NavigationDirection.Left or NavigationDirection.Right
                ? GetIntervalGap(source.Top, source.Bottom, candidate.Top, candidate.Bottom)
                : GetIntervalGap(source.Left, source.Right, candidate.Left, candidate.Right);

            var score = primaryDistance + (crossDistance * 0.1) + (crossGap * 8);
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = index;
            }
        }

        return bestIndex;
    }

    private static Point GetCenter(Rect rect) =>
        new(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));

    private static bool IsInDirection(Rect source, Rect candidate, NavigationDirection direction) =>
        direction switch
        {
            NavigationDirection.Up => candidate.Bottom < source.Center.Y - DirectionTolerance,
            NavigationDirection.Down => candidate.Top > source.Center.Y + DirectionTolerance,
            NavigationDirection.Left => candidate.Right < source.Center.X - DirectionTolerance,
            NavigationDirection.Right => candidate.Left > source.Center.X + DirectionTolerance,
            _ => false
        };

    private static double GetIntervalGap(double firstStart, double firstEnd, double secondStart, double secondEnd)
    {
        if (secondEnd < firstStart)
            return firstStart - secondEnd;
        if (secondStart > firstEnd)
            return secondStart - firstEnd;
        return 0;
    }
}
