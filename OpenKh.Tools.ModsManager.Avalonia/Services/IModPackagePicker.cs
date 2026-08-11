namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IModPackagePicker
{
    Task<string?> PickPackageAsync();
}
