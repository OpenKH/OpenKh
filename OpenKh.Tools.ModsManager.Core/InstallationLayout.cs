namespace OpenKh.Tools.ModsManager.Core;

public sealed class InstallationLayout
{
    private InstallationLayout(string rootDirectory)
    {
        RootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string RootDirectory { get; }
    public string ConfigurationFile => Path.Combine(RootDirectory, "mods-manager.yml");

    public static InstallationLayout Detect(string applicationBaseDirectory, IEnumerable<string>? arguments = null)
    {
        var argumentList = arguments?.ToArray() ?? [];
        for (var index = 0; index < argumentList.Length - 1; index++)
        {
            if (argumentList[index].Equals("--data-root", StringComparison.OrdinalIgnoreCase))
                return new InstallationLayout(argumentList[index + 1]);
        }

        var applicationDirectory = new DirectoryInfo(
            Path.GetFullPath(applicationBaseDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));

        if (applicationDirectory.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) &&
            applicationDirectory.Parent is not null)
        {
            return new InstallationLayout(applicationDirectory.Parent.FullName);
        }

        if (applicationDirectory.Name.Equals("ModManager", StringComparison.OrdinalIgnoreCase) &&
            applicationDirectory.Parent?.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) == true &&
            applicationDirectory.Parent.Parent is not null)
        {
            return new InstallationLayout(applicationDirectory.Parent.Parent.FullName);
        }

        return new InstallationLayout(applicationDirectory.FullName);
    }
}
