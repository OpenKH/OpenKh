#!/bin/sh
# Builds the Linux (Avalonia) Mods Manager and packages it as an AppImage.
#
# Prerequisites: dotnet SDK 8.0.4xx, curl, and FUSE (or run appimagetool with
# APPIMAGE_EXTRACT_AND_RUN=1, which this script sets so it also works in
# containers/CI).
#
# Output: openkh-modsmanager-x86_64.AppImage in the repository root.
set -e

cd "$(dirname "$0")"

# global.json pins an 8.0.4xx SDK; prefer a user-local install (e.g. from
# dotnet-install.sh --channel 8.0.4xx --install-dir ~/.dotnet) if the system
# SDK cannot satisfy it.
if [ -x "$HOME/.dotnet/dotnet" ]; then
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$HOME/.dotnet:$PATH"
fi

PROJECT=OpenKh.Tools.ModsManager.Avalonia
PUBLISH_DIR=publish/linux-x64
APPDIR=publish/AppDir

rm -rf "$PUBLISH_DIR" "$APPDIR"

dotnet publish "$PROJECT/$PROJECT.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:ErrorOnDuplicatePublishOutputFiles=false \
    -o "$PUBLISH_DIR"

# Panacea and its dependencies are native Windows DLLs produced by the
# Windows CI (msbuild of OpenKh.Research.Panacea). The games run under
# Proton, so these same DLLs work on Linux. Fetch them from the latest
# OpenKH release so the wizard's "Install Panacea" works from the AppImage.
PANACEA_DLLS="OpenKH.Panacea.dll
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
swresample-vgmstream-4.dll"

PANACEA_ZIP=publish/openkh-latest.zip
if [ ! -f "$PANACEA_ZIP" ]; then
    curl -fsSL -o "$PANACEA_ZIP" \
        "https://github.com/OpenKH/OpenKh/releases/download/latest/openkh.zip" \
        || rm -f "$PANACEA_ZIP"
fi
if [ -f "$PANACEA_ZIP" ]; then
    for dll in $PANACEA_DLLS; do
        unzip -o -j -q "$PANACEA_ZIP" "openkh/$dll" -d "$PUBLISH_DIR" \
            || echo "warning: $dll not found in the release zip"
    done
else
    echo "warning: could not download openkh.zip; Panacea will not be installable from this build"
fi

# Assemble the AppDir
mkdir -p "$APPDIR/usr/bin"
cp -r "$PUBLISH_DIR"/. "$APPDIR/usr/bin/"
install -m 755 "$PROJECT/packaging/AppRun" "$APPDIR/AppRun"
install -m 644 "$PROJECT/packaging/openkh-modsmanager.desktop" "$APPDIR/openkh-modsmanager.desktop"
install -m 644 "$PROJECT/packaging/openkh-modsmanager.png" "$APPDIR/openkh-modsmanager.png"

# Fetch appimagetool if not present
APPIMAGETOOL=publish/appimagetool
if [ ! -x "$APPIMAGETOOL" ]; then
    curl -fsSL -o "$APPIMAGETOOL" \
        "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"
    chmod +x "$APPIMAGETOOL"
fi

# Unlink first: overwriting fails with "Text file busy" if the previous
# AppImage is still running.
rm -f openkh-modsmanager-x86_64.AppImage

# APPIMAGE_EXTRACT_AND_RUN lets appimagetool run without FUSE (e.g. in CI).
APPIMAGE_EXTRACT_AND_RUN=1 ARCH=x86_64 "$APPIMAGETOOL" "$APPDIR" openkh-modsmanager-x86_64.AppImage

echo "Created openkh-modsmanager-x86_64.AppImage"
