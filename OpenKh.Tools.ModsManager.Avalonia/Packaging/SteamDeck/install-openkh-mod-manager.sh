#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
APPLICATIONS_DIR="$HOME/.local/share/applications"
DESKTOP_FILE="$APPLICATIONS_DIR/openkh-mod-manager.desktop"

mkdir -p "$APPLICATIONS_DIR"
chmod +x "$SCRIPT_DIR/OpenKh.Tools.ModsManager"

{
    printf '%s\n' '[Desktop Entry]'
    printf '%s\n' 'Type=Application'
    printf '%s\n' 'Name=OpenKH Mod Manager'
    printf '%s\n' 'Comment=Manage OpenKH game mods'
    printf 'Exec="%s"\n' "$SCRIPT_DIR/OpenKh.Tools.ModsManager"
    printf 'Path="%s"\n' "$SCRIPT_DIR"
    printf '%s\n' 'Terminal=false'
    printf '%s\n' 'Categories=Game;Utility;'
} > "$DESKTOP_FILE"

echo "OpenKH Mod Manager was added to the application menu."
echo "You can now add it to Steam from Desktop Mode."
