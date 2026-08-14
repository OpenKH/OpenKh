#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
PROJECT="$SCRIPT_DIR/OpenKh.Tools.ModsManager.csproj"
OUTPUT_ROOT="${1:-$SCRIPT_DIR/artifacts}"
PANACEA_LOADER="$SCRIPT_DIR/../OpenKh.Research.Panacea/Release/OpenKH.Panacea.dll"
PANACEA_DEPENDENCIES="$SCRIPT_DIR/../OpenKh.Research.Panacea/Dependencies"
PANACEA_FILES="
avcodec-vgmstream-59.dll
avformat-vgmstream-59.dll
avutil-vgmstream-57.dll
bass.dll
bass_vgmstream.dll
libatrac9.dll
libcelt-0061.dll
libcelt-0110.dll
libg719_decode.dll
libmpg123-0.dll
libspeex-1.dll
libvorbis.dll
swresample-vgmstream-4.dll
"

copy_panacea_files() {
    output="$1"
    if [ ! -f "$PANACEA_LOADER" ]; then
        printf '%s\n' "Warning: Panacea was not built, so it will not be included in $output." >&2
        return
    fi

    cp "$PANACEA_LOADER" "$output/OpenKH.Panacea.dll"
    for file_name in $PANACEA_FILES; do
        source_path="$PANACEA_DEPENDENCIES/$file_name"
        if [ ! -f "$source_path" ]; then
            printf '%s\n' "Required Panacea dependency is missing: $source_path" >&2
            exit 1
        fi
        cp "$source_path" "$output/$file_name"
    done
}

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
    copy_panacea_files "$OUTPUT_ROOT/$folder"
}

publish_target linux-x64 linux-x64
publish_target linux-x64 steam-deck
cp "$SCRIPT_DIR/Packaging/SteamDeck/install-openkh-mod-manager.sh" \
   "$OUTPUT_ROOT/steam-deck/install-openkh-mod-manager.sh"
chmod +x "$OUTPUT_ROOT/steam-deck/OpenKh.Tools.ModsManager" \
         "$OUTPUT_ROOT/steam-deck/install-openkh-mod-manager.sh"
