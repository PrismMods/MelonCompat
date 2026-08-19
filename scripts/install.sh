#!/usr/bin/env bash
# Copies the packed mod into the game's UnityModManager Mods folder and makes sure
# the MelonMods folder mods are loaded from exists.
set -euo pipefail
cd "$(dirname "$0")/.."

GAME="${GAME:-$(sed -n 's|.*<GamePath>\(.*\)</GamePath>.*|\1|p' Directory.Build.props)}"
[ -d "$GAME" ] || { echo "game not found at: $GAME" >&2; exit 1; }

./scripts/pack.sh
mkdir -p "$GAME/Mods" "$GAME/MelonMods"
rm -rf "$GAME/Mods/MelonCompat"
cp -R dist/MelonCompat "$GAME/Mods/MelonCompat"
echo "installed to $GAME/Mods/MelonCompat"
echo "put MelonLoader mods in $GAME/MelonMods"
