#!/usr/bin/env bash
# Produces dist/MelonCompat/ (drop-in UnityModManager mod folder) and a zip of it.
set -euo pipefail
cd "$(dirname "$0")/.."

CONFIG="${CONFIG:-Release}"
OUT="dist/MelonCompat"

dotnet build MelonCompat.Melon/MelonCompat.Melon.csproj -c "$CONFIG" -v q --nologo
dotnet build MelonCompat.Bootstrap/MelonCompat.Bootstrap.csproj -c "$CONFIG" -v q --nologo

rm -rf "$OUT" dist/MelonCompat.zip
mkdir -p "$OUT/Runtime"
cp "MelonCompat.Bootstrap/bin/$CONFIG/MelonCompat.dll" "$OUT/"
cp MelonCompat.Bootstrap/Info.json "$OUT/"
cp "MelonCompat.Melon/bin/$CONFIG/MelonLoader.dll" "$OUT/Runtime/"

(cd dist && zip -qr MelonCompat.zip MelonCompat)
echo "packed $OUT and dist/MelonCompat.zip"
find "$OUT" -type f | sort
