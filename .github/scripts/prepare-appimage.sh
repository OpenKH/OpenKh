#!/bin/sh
set -eu

# linuxdeploy inspects every PATH entry, including inaccessible Windows paths inherited by WSL.
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
export PATH

if [ "$#" -lt 2 ] || [ "$#" -gt 3 ]; then
    echo "Usage: $0 RELEASE_DIRECTORY OUTPUT_APPIMAGE [VERSION]" >&2
    exit 2
fi

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPOSITORY_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)
RELEASE_DIRECTORY=$(CDPATH= cd -- "$1" && pwd)
OUTPUT_DIRECTORY=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd)
OUTPUT_APPIMAGE="$OUTPUT_DIRECTORY/$(basename -- "$2")"
VERSION_NAME="${3:-continuous}"
WORK_DIRECTORY=$(mktemp -d)
APP_DIRECTORY="$WORK_DIRECTORY/OpenKH.AppDir"
TOOLS_DIRECTORY="$WORK_DIRECTORY/tools"
ICON_PATH="$WORK_DIRECTORY/openkh.png"

cleanup() {
    rm -rf "$WORK_DIRECTORY"
}
trap cleanup EXIT INT TERM

require_file() {
    if [ ! -f "$1" ]; then
        echo "Required AppImage input is missing: $1" >&2
        exit 1
    fi
}

require_file "$RELEASE_DIRECTORY/OpenKh.Launcher"
require_file "$RELEASE_DIRECTORY/Apps/OpenKh.Tools.ModsManager"
require_file "$REPOSITORY_ROOT/distribution/AppImage/AppRun"
require_file "$REPOSITORY_ROOT/distribution/AppImage/openkh.desktop"
require_file "$REPOSITORY_ROOT/images/openKH_Old.ico"

mkdir -p \
    "$APP_DIRECTORY/usr/bin" \
    "$APP_DIRECTORY/usr/lib/openkh" \
    "$APP_DIRECTORY/usr/share/applications" \
    "$APP_DIRECTORY/usr/share/icons/hicolor/256x256/apps" \
    "$TOOLS_DIRECTORY"
cp -a "$RELEASE_DIRECTORY/." "$APP_DIRECTORY/usr/lib/openkh/"
chmod +x \
    "$APP_DIRECTORY/usr/lib/openkh/OpenKh.Launcher" \
    "$APP_DIRECTORY/usr/lib/openkh/Apps/OpenKh.Tools.ModsManager"
ln -s ../lib/openkh/OpenKh.Launcher "$APP_DIRECTORY/usr/bin/openkh"
ln -s ../lib/openkh/Apps/OpenKh.Tools.ModsManager "$APP_DIRECTORY/usr/bin/openkh-mod-manager"
install -m 644 \
    "$REPOSITORY_ROOT/distribution/AppImage/openkh.desktop" \
    "$APP_DIRECTORY/usr/share/applications/openkh.desktop"

# The Windows icon already contains the approved OpenKH artwork at several sizes.
convert "${REPOSITORY_ROOT}/images/openKH_Old.ico[0]" \
    -background none \
    -gravity center \
    -resize 256x256 \
    -extent 256x256 \
    "$ICON_PATH"
install -m 644 "$ICON_PATH" "$APP_DIRECTORY/usr/share/icons/hicolor/256x256/apps/openkh.png"

LINUXDEPLOY_VERSION="${LINUXDEPLOY_VERSION:-1-alpha-20251107-1}"
APPIMAGETOOL_VERSION="${APPIMAGETOOL_VERSION:-1.9.1}"
LINUXDEPLOY="$TOOLS_DIRECTORY/linuxdeploy-x86_64.AppImage"
APPIMAGETOOL="$TOOLS_DIRECTORY/appimagetool-x86_64.AppImage"
curl --fail --location --silent --show-error \
    "https://github.com/linuxdeploy/linuxdeploy/releases/download/${LINUXDEPLOY_VERSION}/linuxdeploy-x86_64.AppImage" \
    --output "$LINUXDEPLOY"
curl --fail --location --silent --show-error \
    "https://github.com/AppImage/appimagetool/releases/download/${APPIMAGETOOL_VERSION}/appimagetool-x86_64.AppImage" \
    --output "$APPIMAGETOOL"
chmod +x "$LINUXDEPLOY" "$APPIMAGETOOL"

set -- \
    "$LINUXDEPLOY" \
    --appdir "$APP_DIRECTORY" \
    --desktop-file "$APP_DIRECTORY/usr/share/applications/openkh.desktop" \
    --icon-file "$APP_DIRECTORY/usr/share/icons/hicolor/256x256/apps/openkh.png" \
    --executable "$APP_DIRECTORY/usr/bin/openkh" \
    --executable "$APP_DIRECTORY/usr/bin/openkh-mod-manager"

# Avalonia loads these libraries at runtime, so the .NET app host does not expose them to ldd.
for library_name in libfontconfig.so.1 libfreetype.so.6 libICE.so.6 libSM.so.6 libX11.so.6 libX11-xcb.so.1 libxcb.so.1; do
    library_path=$(ldconfig -p | awk -v name="$library_name" '$1 == name { print $NF; exit }')
    if [ -z "$library_path" ]; then
        echo "Required Linux library was not found: $library_name" >&2
        exit 1
    fi
    set -- "$@" --library "$library_path"
done

APPIMAGE_EXTRACT_AND_RUN=1 "$@"
rm -f "$APP_DIRECTORY/AppRun"
install -m 755 "$REPOSITORY_ROOT/distribution/AppImage/AppRun" "$APP_DIRECTORY/AppRun"

rm -f "$OUTPUT_APPIMAGE"
ARCH=x86_64 \
VERSION="$VERSION_NAME" \
APPIMAGE_EXTRACT_AND_RUN=1 \
    "$APPIMAGETOOL" "$APP_DIRECTORY" "$OUTPUT_APPIMAGE"
chmod +x "$OUTPUT_APPIMAGE"
echo "Created $OUTPUT_APPIMAGE"
