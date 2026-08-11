#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
APPLICATIONS_DIR="$HOME/.local/share/applications"
DESKTOP_FILE="$APPLICATIONS_DIR/openkh.desktop"

chmod +x "$SCRIPT_DIR/OpenKh.Launcher"
chmod +x "$SCRIPT_DIR/Apps/OpenKh.Tools.ModsManager"
mkdir -p "$APPLICATIONS_DIR"

{
    printf '%s\n' '[Desktop Entry]'
    printf '%s\n' 'Type=Application'
    printf '%s\n' 'Name=OpenKH'
    printf '%s\n' 'Comment=Open the OpenKH Launcher'
    printf 'Exec="%s"\n' "$SCRIPT_DIR/OpenKh.Launcher"
    printf 'Path="%s"\n' "$SCRIPT_DIR"
    printf '%s\n' 'Terminal=false'
    printf '%s\n' 'Categories=Game;Utility;'
} > "$DESKTOP_FILE"

echo "OpenKH was added to the application menu."
echo "Steam Deck users can now add it to Steam from Desktop Mode."
