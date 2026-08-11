# OpenKH Launcher

The launcher is the cross-platform entry point for OpenKH releases. It keeps the Mod Manager separate from creator tools and runs on Windows, Linux, and Steam Deck.

## Features

- Opens the packaged Mod Manager without exposing application files to regular users.
- Lists creator tools from the `Apps` directory with search and persistent favorites.
- Checks for OpenKH updates and installs them without opening the Mod Manager first.
- Creates Windows shortcuts and Linux desktop entries.
- Migrates the previous release layout on first launch.
- Supports SDL gamepads throughout the interface.
- Uses Steam launcher mode when Steam Input owns the controller.
- Opens the Steam Deck floating keyboard when a text field receives focus.
- Uses Steam App ID 480 for development and non-Steam testing.

## Publish

```powershell
.\publish.ps1 -Target All
```

The script creates self-contained, single-file builds for Windows, Linux, and Steam Deck.
