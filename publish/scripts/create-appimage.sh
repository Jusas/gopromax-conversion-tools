#!/bin/bash
# Create AppImage for the app (includes compiling)
#set -x
set -e

# Steps
# 1. Copy the AppDir to a new publish folder
# 2. Download Exiftool and extract it under AppDir
# 3. Build the application csproj, and copy the binary to AppDir
# 4. Package the folder to an AppImage using AppImageKit

# Usage:
# ./<script> linux-x64 x86_64 0.9.0.0 /tmp/my/release

# Dotnet architecture string
ARCH="$1"
# Appimagekit architecture string
APPIMAGE_ARCH="$2"
# Version number, will override the CSPROJ number
VERSION="$3"
# Output directory - if left out, will default to under /tmp
OUTPUT_DIR="$4"


SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
NOW=$( date +"%y%m%d%H%M%S" )
TEMPDIR="/tmp/maxvideoconverter-bundle-${ARCH}-${NOW}"
OUTPUT_DIR=${OUTPUT_DIR:-"$TEMPDIR"}
APP_ASSEMBLY_NAME="VideoConversionApp"
FINAL_APP_NAME="MAXVideoConverter"

# Grab the releases from github for these.
EXIFTOOL_VER="13.33"
EXIFTOOL_URL="https://github.com/exiftool/exiftool/archive/refs/tags/${EXIFTOOL_VER}.tar.gz"
APPIMAGEKIT_URL="https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"


echo "** Creating output directory and copying bundle structure there **"
mkdir -p "${OUTPUT_DIR}"
cp -R "${SCRIPT_DIR}/../appimage/MAXVideoConverter.AppDir" "${OUTPUT_DIR}"
APPDIR="${OUTPUT_DIR}/MAXVideoConverter.AppDir"

echo "** Downloading appimagekit and exiftool **"
curl -L -o "${OUTPUT_DIR}/appimagetool" "${APPIMAGEKIT_URL}" && chmod +x "${OUTPUT_DIR}/appimagetool"
curl -L -o "${OUTPUT_DIR}/exiftool.tgz" "${EXIFTOOL_URL}" && tar --directory="${OUTPUT_DIR}" -xf "${OUTPUT_DIR}/exiftool.tgz"

echo "** Moving exiftool files under AppDir **"
mv "${OUTPUT_DIR}/exiftool-${EXIFTOOL_VER}" "${APPDIR}/usr/bin/external-utils"
ln -r -s "${APPDIR}/usr/bin/external-utils/exiftool-${EXIFTOOL_VER}/exiftool" "${APPDIR}/usr/bin/external-utils/exiftool"

echo "** Building application **"
. "${SCRIPT_DIR}/build-singlefile.sh" "${ARCH}" "${VERSION}" "${OUTPUT_DIR}"
cp "${OUTPUT_DIR}/${ARCH}/${APP_ASSEMBLY_NAME}" "${APPDIR}/usr/bin/${FINAL_APP_NAME}"

# APPVER=$(cat "${OUTPUT_DIR}/${ARCH}/version.txt")
APPVER=$VERSION

echo "** Creating AppImage **"
mkdir -p "${OUTPUT_DIR}/release"
ARCH="$APPIMAGE_ARCH" "${OUTPUT_DIR}/appimagetool" "${APPDIR}" "${OUTPUT_DIR}/release/${FINAL_APP_NAME}-${APPVER}-${APPIMAGE_ARCH}.AppImage"

echo "** DONE! **"
