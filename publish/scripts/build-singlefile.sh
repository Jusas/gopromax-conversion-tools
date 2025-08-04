#!/bin/bash
# Build a single-file executable.
# e.g.
# ./<script> linux-x64 /tmp/publish

ARCH="$1"
PUBLISH_ROOT="$2"
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
CSPROJ="${SCRIPT_DIR}/../../src/VideoConversionApp/VideoConversionApp.csproj"

dotnet publish -r "${ARCH}" -c Release --self-contained true -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "${PUBLISH_ROOT}/${ARCH}" "${CSPROJ}"

cat "${CSPROJ}" | grep AssemblyVersion | sed 's%\s*<AssemblyVersion>\(.*\)</AssemblyVersion>%\1%' > "${PUBLISH_ROOT}/${ARCH}/version.txt"
