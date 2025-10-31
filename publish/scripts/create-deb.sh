#!/usr/bin/env bash

set -eo pipefail

usage() { 
  USAGE="$(cat <<EOF
Create a deb package.

$(basename "$0") -p <csproj_file> -a <dotnet_arch> -v <version_string> -d <deb_files_dir> -o <output_dir> [-c]

  -h: Show this help.
  -p: Path to csproj file. Required.
  -a: Build Architecture (dotnet architecture string). Required.
  -v: Version string (1.2.3.4). Required.
  -d: Debian package files dir (with control file, desktop file, icons). Required.
  -o: Output directory for the deb package and the staging directory. Required.
  -c: Clean, cleans the build output directory and deb output directory if
      the flag is set. Optional, defaults to false. 

Example:
  $(basename "$0") -p ../src/Proj.csproj -a linux-x64 -v "1.0.0" -o /tmp/build_out -c

EOF
)"
  echo "$USAGE" >&2; exit 1;
}


while getopts 'a:v:p:d:o:ch' optchar; do 
  case "$optchar" in
    a) arch_string="$OPTARG" ;;
    v) version_string="$OPTARG" ;;
    p) csproj_file="$OPTARG" ;;
    d) deb_files_dir="$OPTARG" ;;
    o) output_dir="$OPTARG" ;;
    c) clean=true ;;
    h|*) usage ;;
  esac 
done
shift $((OPTIND-1))


if [ -z "${arch_string}" ] || [ -z "${version_string}" ] || [ -z "${csproj_file}" ] || [ -z "${output_dir}" ] || [ -z "${deb_files_dir}" ]; then
  echo "Invalid arguments." >&2;
  echo ""
  usage
fi

clean=${clean:-false}
application_name="max-video-converter"
executable_file_name="VideoConversionApp"

set -u
#set -x

echo "Building and packaging as .deb"

build_output_dir="${output_dir}/dotnet-build-out"
deb_output_dir="${output_dir}/deb-out"
echo "- Using output dir: ${build_output_dir}"
echo "- Dotnet project will be built to: ${build_output_dir}"
echo "- Deb package will be built to: ${deb_output_dir}"
echo ""

if [ "$clean" == true ]; then
  echo "Clean flag is true, cleaning directories"
fi

if [ -d "${build_output_dir}" ] && [ "$clean" != true ]; then
  echo "Warning: build output dir already exists but -c flag was not used, build may not be clean."
fi
if [ -d "${build_output_dir}" ] && [ "$clean" == true ]; then
  echo "- Cleaning: deleting dir ${build_output_dir}"
  rm -rf "${build_output_dir}"
fi

if [ -d "${deb_output_dir}" ] && [ "$clean" != true ]; then
  echo "Warning: deb output dir already exists but -c flag was not used, build may not be clean."
fi
if [ -d "${deb_output_dir}" ] && [ "$clean" == true ]; then
  echo "- Cleaning: deleting dir ${deb_output_dir}"  
  rm -rf "${deb_output_dir}"
fi

echo ""
echo "Creating dir structure:"

echo "- ${build_output_dir}"
mkdir -p "${build_output_dir}"
echo "- ${deb_output_dir}"
mkdir -p "${deb_output_dir}"
mkdir -p "${deb_output_dir}/DEBIAN"
mkdir -p "${deb_output_dir}/usr/bin"
mkdir -p "${deb_output_dir}/usr/lib/${application_name}"
mkdir -p "${deb_output_dir}/usr/share/applications"
mkdir -p "${deb_output_dir}/usr/share/pixmaps"
mkdir -p "${deb_output_dir}/usr/share/icons/hicolor/scalable/apps"

echo ""
echo "Building the project..."

dotnet clean "${csproj_file}"
dotnet publish \
  --verbosity quiet \
  --nologo \
  --configuration Release \
  -p:Version="${version_string}" \
  -p:DebugType=embedded \
  --self-contained true \
  --runtime "${arch_string}" \
  --output "${build_output_dir}/${arch_string}" \
  "${csproj_file}"


echo "Build complete"
echo ""

echo "Packaging to .deb"
echo "- Copying files"

# Control file
cp "${deb_files_dir}/control" "${deb_output_dir}/DEBIAN"
sed -i -e "s/BUILD_VERSION/${version_string}/" "${deb_output_dir}/DEBIAN/control"

# Starter script
cat <<EOF > "${deb_output_dir}/usr/bin/${application_name}"
#!/usr/bin/env bash
exec /usr/lib/${application_name}/${executable_file_name} "\$@"
EOF
chmod +x "${deb_output_dir}/usr/bin/${application_name}"

# Binaries
cp -f -a "${build_output_dir}/${arch_string}/." "${deb_output_dir}/usr/lib/${application_name}/"
chmod -R a+rX "${deb_output_dir}/usr/lib/${application_name}/"
chmod +x "${deb_output_dir}/usr/lib/${application_name}/${executable_file_name}"

# Desktop file
cp ${deb_files_dir}/*.desktop "${deb_output_dir}/usr/share/applications"

# Icons
cp ${deb_files_dir}/*.png "${deb_output_dir}/usr/share/pixmaps"
cp ${deb_files_dir}/*.svg "${deb_output_dir}/usr/share/icons/hicolor/scalable/apps"

deb_file_name="${output_dir}/${application_name}_${version_string}_amd64.deb"
echo "- Building deb to: ${output_dir}/${application_name}_${version_string}_amd64.deb"

dpkg-deb --root-owner-group --build "${deb_output_dir}" "${deb_file_name}"

echo ""
echo "** DONE! **"


