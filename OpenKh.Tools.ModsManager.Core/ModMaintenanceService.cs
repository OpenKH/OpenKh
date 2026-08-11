using LibGit2Sharp;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModMaintenanceService(ModManagerConfigurationService configuration)
{
    public Task<int> CheckForUpdateAsync(
        ModEntry mod,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => CheckForUpdate(mod, cancellationToken), cancellationToken);

    public Task UpdateAsync(
        ModEntry mod,
        GameInfo game,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Update(mod, game, progress, cancellationToken), cancellationToken);

    public Task RemoveAsync(ModEntry mod, CancellationToken cancellationToken = default) =>
        Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteDirectory(mod.Directory);
        }, cancellationToken);

    private static int CheckForUpdate(ModEntry mod, CancellationToken cancellationToken)
    {
        if (!Repository.IsValid(mod.Directory))
            return -1;

        using var repository = new Repository(mod.Directory);
        if (repository.Info.IsHeadDetached || string.IsNullOrWhiteSpace(repository.Head.RemoteName))
            return -1;

        Fetch(repository, null, cancellationToken);
        return repository.Head.TrackingDetails.BehindBy ?? 0;
    }

    private void Update(
        ModEntry mod,
        GameInfo game,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!Repository.IsValid(mod.Directory))
            throw new InvalidOperationException("This mod was installed from a local file and cannot be updated from a repository.");

        using (var repository = new Repository(mod.Directory))
        {
            if (repository.Info.IsHeadDetached || repository.Head.TrackedBranch?.Tip is null)
                throw new InvalidOperationException("This repository does not track a remote branch.");

            progress?.Report(new ModOperationProgress($"Fetching {mod.Name}"));
            Fetch(repository, progress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            repository.Reset(ResetMode.Hard, repository.Head.TrackedBranch.Tip, new CheckoutOptions
            {
                CheckoutModifiers = CheckoutModifiers.Force,
                OnCheckoutProgress = (path, completed, total) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(new ModOperationProgress(
                        string.IsNullOrWhiteSpace(path) ? $"Updating {mod.Name}" : path,
                        total > 0 ? (double)completed / total : null));
                }
            });
            SubmoduleUpdateOptions submoduleOptions = new() { Init = true };
            foreach (var submodule in repository.Submodules)
                repository.Submodules.Update(submodule.Name, submoduleOptions);
        }

        var metadata = ModMetadata.Read(Path.Combine(mod.Directory, "mod.yml"));
        if (metadata.IsCollection == mod.IsCollection)
            return;

        var destination = metadata.IsCollection
            ? Path.Combine(configuration.CollectionsDirectory, mod.Id)
            : Path.Combine(configuration.GetGameModsDirectory(game), mod.Id);
        if (Directory.Exists(destination))
            throw new IOException($"Cannot move the updated mod because '{destination}' already exists.");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        Directory.Move(mod.Directory, destination);
    }

    private static void Fetch(
        Repository repository,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var remote = repository.Network.Remotes[repository.Head.RemoteName];
        if (remote is null)
            throw new InvalidOperationException("The tracked remote no longer exists.");

        var options = new FetchOptions
        {
            OnProgress = message =>
            {
                progress?.Report(new ModOperationProgress(message.Trim()));
                return !cancellationToken.IsCancellationRequested;
            },
            OnTransferProgress = transfer =>
            {
                progress?.Report(new ModOperationProgress(
                    "Receiving repository objects",
                    transfer.TotalObjects > 0 ? (double)transfer.ReceivedObjects / transfer.TotalObjects : null));
                return !cancellationToken.IsCancellationRequested;
            }
        };
        Commands.Fetch(repository, remote.Name, remote.FetchRefSpecs.Select(spec => spec.Specification), options, null);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void DeleteDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(directory, true);
    }
}
