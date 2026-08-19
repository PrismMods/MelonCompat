#!/usr/bin/env bash
# Checks a MelonLoader mod against the shim and lists anything MelonCompat is
# missing, before you find out the hard way in-game.
#   ./scripts/check-mod.sh path/to/Mod.dll
set -euo pipefail
cd "$(dirname "$0")/.."
[ $# -ge 1 ] || { echo "usage: $0 <mod.dll> [more.dll ...]" >&2; exit 2; }

dotnet build MelonCompat.Melon/MelonCompat.Melon.csproj -c Release -v q --nologo >/dev/null
status=0
for mod in "$@"; do
  dotnet run --project tools/RefScan -c Release -v q -- \
    "$mod" MelonCompat.Melon/bin/Release/MelonLoader.dll || status=1
done
exit $status
