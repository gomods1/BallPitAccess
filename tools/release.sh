#!/bin/sh
# Stage a release: build the mod, check the strings, rebuild the READMEs and copy what ships into
# release/payload (committed; GitHub cannot build the mod, it references the game's own assemblies).
# Then:  git commit -am "Release X.Y.Z" && git tag vX.Y.Z && git push --follow-tags
# The Release workflow adds MelonLoader 0.7.3 and Prism 0.18.3 (official downloads, checked by
# SHA-256), zips it as BallPitAccess-vX.Y.Z.zip and attaches installer/BallPitAccessibilityInstaller.exe.
# The installer is built in ../installer (build_installer.bat) and only needs copying when it changes:
#   tools/release.sh installer
set -e
ROOT=$(cd "$(dirname "$0")/.." && pwd)
cd "$ROOT"

version=$(sed -n 's/.*MelonInfo(typeof(BallPitAccess.Main), "BALL x PIT Access", "\([0-9.]*\)".*/\1/p' src/BallPitAccess/Main.cs)
[ -n "$version" ] || { echo "No version in Main.cs" >&2; exit 1; }
grep -q "^## $version\$" CHANGELOG.md || { echo "CHANGELOG.md has no '## $version' section" >&2; exit 1; }
for f in README*.md; do
  head -12 "$f" | grep -q "$version" || { echo "$f does not name version $version" >&2; exit 1; }
done

python tools/check_loc.py
python tools/build_readme.py

OUT=$(dotnet build src/BallPitAccess -c Release 2>&1) || { echo "$OUT" | grep -E " error " | sort -u; echo BUILD FAILED; exit 1; }

P=release/payload
rm -rf "$P"
mkdir -p "$P/Mods" "$P/BallPitAccess"
cp src/BallPitAccess/bin/Release/BallPitAccess.dll "$P/Mods/"
cp README*.html "$P/BallPitAccess/"

if [ "$1" = installer ]; then
  mkdir -p installer
  cp ../installer/target/release/BallPitAccessibilityInstaller.exe installer/
fi
[ -f installer/BallPitAccessibilityInstaller.exe ] || { echo "installer/BallPitAccessibilityInstaller.exe is missing: run tools/release.sh installer" >&2; exit 1; }

echo "Staged $version:"
find "$P" -type f | sort
