#!/bin/sh
# Build the mod, stop the game, copy the DLLs, relaunch. Usage: tools/deploy.sh [nolaunch]
set -e
ROOT=/c/Users/Nitropc/code/mods/ballpit
G=/s/steam/steamapps/common/BALLxPIT
OUT=$(dotnet build "$ROOT/src/BallPitAccess" -c Release 2>&1); echo "$OUT" | grep -E " error " | sort -u; echo "$OUT" | grep -qE " 0 Errores| 0 Error" || { echo BUILD FAILED; exit 1; }
powershell -NoProfile -Command "Get-Process Balls -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 2"
mkdir -p "$G/Mods"
[ -f "$G/prism.dll" ] || cp /c/Users/Nitropc/code/libs/prism/prism-windows-x64/dynamic/release/bin/prism.dll "$G/"
rm -f "$G/UserData/BallPitAccess/speech.log"
cp "$ROOT/src/BallPitAccess/bin/Release/BallPitAccess.dll" "$G/Mods/"
echo "deployed $(date +%T)"
[ "$1" = nolaunch ] && exit 0
powershell -NoProfile -ExecutionPolicy Bypass -File "$ROOT/tools/launch.ps1"
