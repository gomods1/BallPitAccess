#!/bin/sh
# Wait until the speech log contains a regex (default timeout 90 s). Prints the new speech lines.
# Usage: tools/waitfor.sh <regex> [timeout_s]
F=/s/steam/steamapps/common/BALLxPIT/UserData/BallPitAccess/speech.log
T=${2:-90}; i=0
while [ $i -lt $T ]; do
  if grep -qE "$1" "$F" 2>/dev/null; then tail -n 6 "$F"; exit 0; fi
  sleep 1; i=$((i+1))
done
echo "TIMEOUT waiting for $1"; tail -n 6 "$F"; exit 1
