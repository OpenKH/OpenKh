using Avalonia;
using Avalonia.Input;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public class SpatialNavigationTests
{
    [Fact]
    public void BrowseGridMovesDownWithinTheSameColumn()
    {
        var source = new Rect(200, 0, 180, 220);
        var candidates = new[]
        {
            new Rect(0, 240, 180, 220),
            new Rect(200, 240, 180, 220),
            new Rect(400, 240, 180, 220)
        };

        Assert.Equal(1, SpatialNavigation.FindNearest(source, candidates, NavigationDirection.Down));
    }

    [Theory]
    [InlineData(NavigationDirection.Left, 0)]
    [InlineData(NavigationDirection.Right, 1)]
    [InlineData(NavigationDirection.Up, 2)]
    [InlineData(NavigationDirection.Down, 3)]
    public void GridMovesToTheNearestControlInEveryDirection(NavigationDirection direction, int expected)
    {
        var source = new Rect(200, 200, 180, 180);
        var candidates = new[]
        {
            new Rect(0, 200, 180, 180),
            new Rect(400, 200, 180, 180),
            new Rect(200, 0, 180, 180),
            new Rect(200, 400, 180, 180)
        };

        Assert.Equal(expected, SpatialNavigation.FindNearest(source, candidates, direction));
    }

    [Fact]
    public void MainMenuCanReachTheInstalledModList()
    {
        var menuButton = new Rect(0, 300, 220, 60);
        var candidates = new[]
        {
            new Rect(300, 0, 800, 60),
            new Rect(300, 240, 600, 180),
            new Rect(950, 650, 280, 60)
        };

        Assert.Equal(1, SpatialNavigation.FindNearest(menuButton, candidates, NavigationDirection.Right));
    }

    [Fact]
    public void SetupUsesHorizontalAndVerticalNeighbours()
    {
        var folderInput = new Rect(0, 100, 800, 50);
        var candidates = new[]
        {
            new Rect(820, 100, 120, 50),
            new Rect(0, 180, 220, 50),
            new Rect(820, 180, 120, 50)
        };

        Assert.Equal(0, SpatialNavigation.FindNearest(folderInput, candidates, NavigationDirection.Right));
        Assert.Equal(1, SpatialNavigation.FindNearest(folderInput, candidates, NavigationDirection.Down));
    }

    [Fact]
    public void BrowseGridCanReachTheDetailsAction()
    {
        var rightmostCard = new Rect(400, 0, 180, 220);
        var candidates = new[]
        {
            new Rect(0, 240, 180, 220),
            new Rect(200, 240, 180, 220),
            new Rect(400, 240, 180, 220),
            new Rect(700, 500, 320, 60)
        };

        Assert.Equal(3, SpatialNavigation.FindNearest(rightmostCard, candidates, NavigationDirection.Right));
    }
}
