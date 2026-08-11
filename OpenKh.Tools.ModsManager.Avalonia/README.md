# OpenKH Mod Manager for Avalonia

This project is the cross-platform replacement for the WPF Mod Manager. It runs alongside the current application while features are migrated and verified.

## Current functionality

- Uses the existing `mods-manager.yml` and enabled-mod list files.
- Lists local mods and collections for every supported game.
- Searches by title, author, or repository.
- Enables and disables mods with automatic saving.
- Changes mod priority.
- Installs local ZIP packages with safe path validation.
- Opens the selected mod directory.
- Provides a layout designed for desktop displays and the Steam Deck screen.

Remote installation, update, extraction, patching, and game launch remain in the WPF application until their services have been separated from Windows UI dependencies.

## Run locally

```powershell
dotnet run --project OpenKh.Tools.ModsManager.Avalonia.csproj -- --data-root E:\path\to\OpenKH
```

The `--data-root` argument is optional in packaged builds. An executable placed in the `Apps` directory automatically uses the parent OpenKH installation directory.

## Publish

On Windows:

```powershell
.\publish.ps1 -Target All
```

On Linux:

```sh
./publish.sh
```

Both scripts create self-contained, single-file builds. The Steam Deck package includes a helper that adds the Mod Manager to the Desktop Mode application menu.
