#!/bin/bash
# Build a single-file executable.
# e.g.
# ./<script> linux-x64 0.9.0.0 /tmp/publish

ARCH="$1"
VERSION="$2"
PUBLISH_ROOT="$3"

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
CSPROJ="${SCRIPT_DIR}/../../src/VideoConversionApp/VideoConversionApp.csproj"

dotnet clean "${CSPROJ}"
dotnet publish -r "${ARCH}" -c Release  --self-contained true -p:Version=$VERSION -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "${PUBLISH_ROOT}/${ARCH}" "${CSPROJ}"

# cat "${CSPROJ}" | grep AssemblyVersion | sed 's%\s*<AssemblyVersion>\(.*\)</AssemblyVersion>%\1%' > "${PUBLISH_ROOT}/${ARCH}/version.txt"
# echo "${VERSION}" > "${PUBLISH_ROOT}/${ARCH}/version.txt"
