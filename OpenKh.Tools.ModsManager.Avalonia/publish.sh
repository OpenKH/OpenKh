#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
PROJECT="$SCRIPT_DIR/OpenKh.Tools.ModsManager.Avalonia.csproj"
OUTPUT_ROOT="${1:-$SCRIPT_DIR/artifacts}"

publish_target() {
    runtime="$1"
    folder="$2"
    dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$runtime" \
        --self-contained true \
        --source "https://api.nuget.org/v3/index.json" \
        --output "$OUTPUT_ROOT/$folder" \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:DebugType=None \
        -p:DebugSymbols=false
}

publish_target linux-x64 linux-x64
publish_target linux-x64 steam-deck
cp "$SCRIPT_DIR/Packaging/SteamDeck/install-openkh-mod-manager.sh" \
   "$OUTPUT_ROOT/steam-deck/install-openkh-mod-manager.sh"
chmod +x "$OUTPUT_ROOT/steam-deck/OpenKh.ModManager" \
         "$OUTPUT_ROOT/steam-deck/install-openkh-mod-manager.sh"
