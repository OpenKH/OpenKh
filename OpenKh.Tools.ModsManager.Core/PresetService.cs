namespace OpenKh.Tools.ModsManager.Core;

public sealed class PresetService
{
    private readonly string _presetDirectory;

    public PresetService(ModManagerConfigurationService configuration)
    {
        _presetDirectory = Path.Combine(configuration.InstallationDirectory, "presets");
        Directory.CreateDirectory(_presetDirectory);
    }

    public IReadOnlyList<string> GetNames() => Directory.EnumerateFiles(_presetDirectory, "*.txt")
        .Select(Path.GetFileNameWithoutExtension)
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Cast<string>()
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public void Save(string name, IEnumerable<string> enabledModIds)
    {
        var safeName = SanitizeName(name);
        File.WriteAllLines(GetPath(safeName), enabledModIds);
    }

    public IReadOnlyList<string> Load(string name)
    {
        var path = GetPath(name);
        if (!File.Exists(path))
            throw new FileNotFoundException("The selected preset could not be found.", path);
        return File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
    }

    public void Remove(string name)
    {
        var path = GetPath(name);
        if (File.Exists(path))
            File.Delete(path);
    }

    private string GetPath(string name) => Path.Combine(_presetDirectory, $"{SanitizeName(name)}.txt");

    private static string SanitizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Enter a preset name.", nameof(name));
        var result = string.Join("+", name.Trim().Split(Path.GetInvalidFileNameChars()));
        return string.IsNullOrWhiteSpace(result) ? "Preset" : result;
    }
}
