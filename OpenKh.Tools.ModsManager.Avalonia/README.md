# OpenKH Mod Manager

This is the official cross-platform OpenKH Mod Manager for Windows, Linux, and Steam Deck. Its interface uses Avalonia so the same application can run natively on every supported platform.

## Current functionality

- Uses the existing `mods-manager.yml` and enabled-mod list files.
- Lists local mods and collections for every supported game.
- Searches by title, author, or repository.
- Enables and disables mods with automatic saving.
- Changes mod priority.
- Installs local packages, repository URLs, ZIP URLs, Lua files, and mods from the official online catalog.
- Shows online and installed mod icons, previews, authors, descriptions, and repository information.
- Installs, removes, updates, and configures mods and collections.
- Saves and loads mod presets.
- Extracts game data and configures PC, Steam, Epic, and PCSX2 installations.
- Installs and removes Panacea, the Steam App ID helper, and Lua Backend.
- Builds mods, patches or restores PC packages, and launches or stops supported games.
- Provides creator workflows for metadata, target-file search, copy assets, preferences, and diff previews.
- Checks the complete OpenKH package for updates.
- Supports SDL gamepads with controller-specific PlayStation, Nintendo, and Xbox labels.
- Uses Steam launcher mode when Steam Input owns the controller instead of exposing it to SDL.
- Opens the Steam Deck floating keyboard when a text field receives focus.
- Opens the selected mod directory.
- Provides responsive layouts designed for Windows, Linux, and the Steam Deck screen.

## Run locally

```powershell
dotnet run --project OpenKh.Tools.ModsManager.csproj -- --data-root E:\path\to\OpenKH
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
