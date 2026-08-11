namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Author { get; init; } = "Unknown author";
    public string Description { get; init; } = "No description is available for this mod.";
    public required string Directory { get; init; }
    public string? IconPath { get; init; }
    public string? PreviewPath { get; init; }
    public bool IsCollection { get; init; }
    public bool IsEnabled { get; set; }
    public int UpdateCount { get; set; }
}
