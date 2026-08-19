#!/usr/bin/env bash
# Builds the shim, the sample mod, and the Mono test harness, then runs the
# harness against a scratch copy of everything it needs.
set -euo pipefail
cd "$(dirname "$0")/.."

GAME="${GAME:-$(sed -n 's|.*<GamePath>\(.*\)</GamePath>.*|\1|p' Directory.Build.props)}"
UMM="$GAME/ADanceOfFireAndIce.app/Contents/Resources/Data/Managed/UnityModManager"
[ -d "$UMM" ] || UMM="$GAME/ADOFAI_Data/Managed/UnityModManager"

dotnet build MelonCompat.Melon/MelonCompat.Melon.csproj -c Release -v q --nologo
dotnet build samples/SampleMelonMod/SampleMelonMod.csproj -c Release -v q --nologo

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
cp MelonCompat.Melon/bin/Release/MelonLoader.dll "$WORK/"
cp samples/SampleMelonMod/bin/Release/SampleMelonMod.dll "$WORK/"
cp "$UMM/0Harmony.dll" "$UMM/dnlib.dll" "$WORK/"

mcs tests/TranslationTest.cs -out:"$WORK/TranslationTest.exe" -r:System.Core >/dev/null
mono "$WORK/TranslationTest.exe" "$WORK"
