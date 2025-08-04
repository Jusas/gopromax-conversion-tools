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
# ./<script> linux-x64 x86_64
ARCH="$1"
APPIMAGE_ARCH="$2"

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
NOW=$( date +"%y%m%d%H%M%S" )
TEMPDIR="/tmp/maxvideoconverter-bundle-${ARCH}-${NOW}"
APP_ASSEMBLY_NAME="VideoConversionApp"
FINAL_APP_NAME="MAXVideoConverter"


EXIFTOOL_VER="13.33"
EXIFTOOL_URL="https://github.com/exiftool/exiftool/archive/refs/tags/${EXIFTOOL_VER}.tar.gz"
APPIMAGEKIT_URL="https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"


echo "** Creating temporary directory and copying bundle structure there **"
mkdir -p "${TEMPDIR}"
cp -R "${SCRIPT_DIR}/../appimage/MAXVideoConverter.AppDir" "${TEMPDIR}"
APPDIR="${TEMPDIR}/MAXVideoConverter.AppDir"

echo "** Downloading appimagekit and exiftool **"
curl -L -o "${TEMPDIR}/appimagetool" "${APPIMAGEKIT_URL}" && chmod +x "${TEMPDIR}/appimagetool"
curl -L -o "${TEMPDIR}/exiftool.tgz" "${EXIFTOOL_URL}" && tar --directory="${TEMPDIR}" -xf "${TEMPDIR}/exiftool.tgz"

echo "** Moving exiftool files under AppDir **"
mv "${TEMPDIR}/exiftool-${EXIFTOOL_VER}" "${APPDIR}/usr/bin/external-utils"
ln -r -s "${APPDIR}/usr/bin/external-utils/exiftool-${EXIFTOOL_VER}/exiftool" "${APPDIR}/usr/bin/external-utils/exiftool"

echo "** Building application **"
. "${SCRIPT_DIR}/build-singlefile.sh" "${ARCH}" "${TEMPDIR}"
cp "${TEMPDIR}/${ARCH}/${APP_ASSEMBLY_NAME}" "${APPDIR}/usr/bin/${FINAL_APP_NAME}"

APPVER=$(cat "${TEMPDIR}/${ARCH}/version.txt")

echo "** Creating AppImage **"
mkdir -p "${TEMPDIR}/release"
ARCH="$APPIMAGE_ARCH" "${TEMPDIR}/appimagetool" "${APPDIR}" "${TEMPDIR}/release/${FINAL_APP_NAME}-${APPVER}-${APPIMAGE_ARCH}.AppImage"

echo "** DONE! **"
