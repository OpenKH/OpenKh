using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaModPackagePicker(Window owner) : IModPackagePicker
{
    public async Task<string?> PickPackageAsync()
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Install an OpenKH mod",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("OpenKH mod packages")
                {
                    Patterns = ["*.zip"]
                }
            ]
        });

        return files.Count == 0 ? null : files[0].TryGetLocalPath();
    }
}
