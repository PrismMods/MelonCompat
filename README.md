<div align="center">
<p>
    <img src="https://capsule-render.vercel.app/api?type=waving&color=C8BEB2&height=300&section=header&text=MelonCompat&fontSize=90&animation=fadeIn&fontAlignY=38&desc=An%20all%20in%20one%20mod%20for%20A%20Dance%20of%20Fire%20and%20Ice.&descAlignY=55&descAlign=62%22"/>
</p>

[![Latest Release](https://img.shields.io/github/v/release/PrismMods/MelonCompat?include_prereleases&sort=date&label=release&logo=github)](https://github.com/PrismMods/MelonCompat/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/PrismMods/MelonCompat/total?logo=github&label=downloads)](https://github.com/PrismMods/MelonCompat/releases)
[![Stars](https://img.shields.io/github/stars/PrismMods/MelonCompat?logo=github)](https://github.com/PrismMods/MelonCompat/stargazers)
[![Discord](https://img.shields.io/discord/1499236885409566891?logo=discord&logoColor=white&label=Discord&color=5865F2)](https://discord.gg/mAzAghu5Xq)
</div>

Run **MelonLoader mods** in *A Dance of Fire and Ice* without MelonLoader — MelonCompat is a UnityModManager mod that ships a drop-in `MelonLoader.dll` shim, so Melon mods load side by side with your usual UMM mods.

> [!NOTE]
> Mods that use an IL manipulator won't work.

---

## Install

1. Download **MelonCompat** from the [releases page](https://github.com/PrismMods/MelonCompat/releases/latest) and install it like any other UnityModManager mod.
2. Launch ADOFAI once — a `MelonMods` folder appears in your ADOFAI folder.
3. Done.

## Installing Melon mods

Most ADOFAI Melon mod zips look something like this:

```
MLMods/
├── Mods/
├── UserData/
└── UserLibs/
```

1. Rename `Mods` → `MelonMods`.
2. Drag the folders into the root of your ADOFAI folder:

```
A Dance of Fire and Ice/
├── Mods/          UnityModManager mods (MelonCompat lives here)
├── MelonMods/     MelonLoader mods go here
├── UserData/      mod settings and saves
└── UserLibs/      shared dependencies
```

> [!TIP]
> Folder names vary between zips. Whatever the mod `.dll` files sit in is the folder to rename to `MelonMods`.

Dependencies are found beside the mod, in the mod's own subfolder, or in `UserLibs` — all three are searched automatically.

## What works

| | |
|---|---|
| ✅ | `MelonMod` lifecycle — `OnInitializeMelon`, `OnUpdate`, `OnFixedUpdate`, `OnLateUpdate`, `OnGUI`, `OnApplicationQuit` |
| ✅ | Scene events, `MelonCoroutines`, `MelonLogger`, `MelonEvents` |
| ✅ | `MelonPreferences` with TOML files in `UserData` |
| ✅ | HarmonyX patching, bridged onto UnityModManager's Harmony |
| ❌ | Mods using an IL manipulator |
| ❌ | IL2CPP-only mods — ADOFAI is Mono |

Running under the real MelonLoader? MelonCompat detects it and stops, so nothing loads twice.

## Build from source

Requires the .NET SDK (see [`global.json`](global.json)). Point `<GamePath>` in [`Directory.Build.props`](Directory.Build.props) at your ADOFAI install first.

```bash
./scripts/pack.sh       # -> dist/MelonCompat/ + dist/MelonCompat.zip
./scripts/install.sh    # pack, then copy into the game's Mods folder
./scripts/run-tests.sh  # build the shim + sample mod, run the Mono harness
```

Check a Melon mod against the shim before running it in-game — it lists everything MelonCompat is missing:

```bash
./scripts/check-mod.sh path/to/Mod.dll
```

## Project layout

```
MelonCompat.Bootstrap/   thin UMM entry point + assembly redirector
MelonCompat.Melon/       the shim itself, built as MelonLoader.dll
  ├── Api/               MelonMod, MelonLogger, MelonEvents, ...
  ├── Prefs/             MelonPreferences and the TOML reader
  └── Support/           Harmony bridge, Unity host, probe paths
samples/SampleMelonMod/  minimal Melon mod used by the tests
tools/RefScan/           compatibility scanner behind check-mod.sh
```

## License

[MIT](LICENSE)

<a href="https://www.star-history.com/?repos=PrismMods%2FMelonCompat&type=date&legend=top-left">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/chart?repos=PrismMods/MelonCompat&type=date&theme=dark&legend=top-left&sealed_token=0z2fKEOscuETx77g_pahKCXk2pq9kb2k02w_yGKj6QWpw_3xyRYOOw37sPiXFqOMAaEWe-yYfIkDcad_ZX5PgYsArOlVTU429gHT7ZghnS0Tc3iL6zsiHQ" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/chart?repos=PrismMods/MelonCompat&type=date&legend=top-left&sealed_token=0z2fKEOscuETx77g_pahKCXk2pq9kb2k02w_yGKj6QWpw_3xyRYOOw37sPiXFqOMAaEWe-yYfIkDcad_ZX5PgYsArOlVTU429gHT7ZghnS0Tc3iL6zsiHQ" />
   <img alt="Star History Chart" src="https://api.star-history.com/chart?repos=PrismMods/MelonCompat&type=date&legend=top-left&sealed_token=0z2fKEOscuETx77g_pahKCXk2pq9kb2k02w_yGKj6QWpw_3xyRYOOw37sPiXFqOMAaEWe-yYfIkDcad_ZX5PgYsArOlVTU429gHT7ZghnS0Tc3iL6zsiHQ" />
 </picture>
</a>
