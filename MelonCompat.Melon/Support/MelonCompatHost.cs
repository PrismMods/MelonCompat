using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader.Utils;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace MelonLoader.Support;

// Drives the whole thing: find the mods, bring them up in MelonLoader's order,
// and keep feeding them the callbacks MelonLoader would have.
public static class MelonCompatHost {
    private static UnityModManager.ModEntry entry;
    private static bool started;
    private static int pendingSceneBuildIndex = -1;
    private static string pendingSceneName;

    // Scene and update dispatch runs every frame, so the typed views over the
    // registry are cached and rebuilt only when the registry or a mod's enabled
    // state changes.
    private static MelonBase[] active = [];
    private static MelonMod[] mods = [];
    private static MelonPlugin[] plugins = [];

    public static bool Load(UnityModManager.ModEntry modEntry, Action<string> addProbePath) {
        entry = modEntry;
        LogSink.Bind(modEntry.Logger.Log, modEntry.Logger.Warning, modEntry.Logger.Error);

        modEntry.OnUnload = OnUnload;
        modEntry.OnSaveGUI = _ => MelonPreferences.Save();

        try {
            Start(addProbePath);
            return true;
        } catch(Exception e) {
            modEntry.Logger.Error($"MelonCompat failed to start: {e}");
            return false;
        }
    }

    private static void Start(Action<string> addProbePath) {
        // modsPath is <game root>/Mods, so its parent is the install root — the
        // same folder MelonLoader would have used.
        string ummMods = UnityModManager.modsPath;
        if(!string.IsNullOrEmpty(ummMods))
            MelonEnvironment.SetGameRootDirectory(Directory.GetParent(ummMods.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))?.FullName);

        string modsDirectory = MelonEnvironment.ModsDirectory;
        Directory.CreateDirectory(modsDirectory);
        Directory.CreateDirectory(MelonEnvironment.UserDataDirectory);

        // Mods ship their dependencies beside themselves, in their own subfolder,
        // or in UserLibs. All three become probe paths before anything is loaded.
        ProbePaths.Bind(addProbePath);
        addProbePath?.Invoke(modsDirectory);
        addProbePath?.Invoke(MelonEnvironment.UserLibsDirectory);
        foreach(string directory in Directory.GetDirectories(modsDirectory)) addProbePath?.Invoke(directory);

        MelonLogger.Msg($"MelonCompat is loading MelonLoader mods from {modsDirectory}");
        MelonLogger.Msg($"game root {MelonEnvironment.GameRootDirectory} (dataPath {UnityEngine.Application.dataPath})");

        UnityHost.Ensure();
        UnityHost.OnUpdateCallback = OnUpdate;
        UnityHost.OnFixedUpdateCallback = OnFixedUpdate;
        UnityHost.OnLateUpdateCallback = OnLateUpdate;
        UnityHost.OnGuiCallback = OnGui;
        UnityHost.OnQuitCallback = OnApplicationQuit;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        MelonBase.OnMelonRegistered.Subscribe(_ => RefreshViews());
        MelonBase.OnMelonUnregistered.Subscribe(_ => RefreshViews());
        UmmModList.Changed += RefreshViews;

        MelonEvents.OnPreInitialization.Invoke();
        MelonBase.Execute(p => p.OnPreInitialization(), plugins);

        LoadUserLibs();

        string[] files = FindModFiles(modsDirectory);
        foreach(string file in files) MelonAssembly.LoadMelonAssembly(file);

        MelonPreferences.Load();

        MelonEvents.OnApplicationEarlyStart.Invoke();
        MelonBase.Execute(p => p.OnApplicationEarlyStart(), plugins);
        MelonEvents.OnPreModsLoaded.Invoke();
        MelonBase.Execute(p => p.OnPreModsLoaded(), plugins);

        // Patches go on before any mod's initialize callback runs, matching
        // MelonLoader's own ordering.
        MelonEvents.MelonHarmonyInit.Invoke();

        // Anything that had to be applied by rewriting call sites is done once
        // here, after every mod has registered, so the assemblies are scanned a
        // single time.
        ExternPatcher.InstallRedirects(new HarmonyLib.Harmony("meloncompat.externredirect"));

        MelonEvents.OnApplicationStart.Invoke();
        MelonBase.ExecuteAll(m => {
            m.OnInitializeMelon();
            m.OnApplicationStart();
        }, true, "it threw while initializing");

        int count = MelonBase.RegisteredMelons.Count;
        MelonLogger.Msg($"{count} {MelonUtils.MakePlural("mod", count)} loaded from {files.Length} {MelonUtils.MakePlural("file", files.Length)}");
    }

    // A mod switched off in the UnityModManager list keeps its registration but
    // stops receiving frame and scene callbacks, so it costs nothing per frame.
    private static void RefreshViews() {
        active = MelonBase.Snapshot.Where(UmmModList.IsEnabled).ToArray();
        mods = active.OfType<MelonMod>().ToArray();
        plugins = active.OfType<MelonPlugin>().ToArray();
    }

    // MelonLoader accepts both Mods/Thing.dll and Mods/Thing/Thing.dll, and no
    // deeper. Going deeper would try to load every bundled dependency as a mod and
    // fill the log with complaints about libraries that were never meant to be one;
    // those folders are still on the probe path, so they resolve when referenced.
    private static string[] FindModFiles(string modsDirectory) {
        List<string> files = [.. Directory.GetFiles(modsDirectory, "*.dll")];
        foreach(string directory in Directory.GetDirectories(modsDirectory)) {
            string name = Path.GetFileName(directory);
            if(LibraryFolders.Contains(name)) continue;
            files.AddRange(Directory.GetFiles(directory, "*.dll"));
        }
        return files
            .Where(f => !Path.GetFileName(f).StartsWith(".", StringComparison.Ordinal))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    // MelonLoader loads every managed UserLibs assembly up front, not on demand.
    // Plugin-style libraries (e.g. Overlayer modules) are never referenced by the
    // mod that consumes them — it finds them by scanning the AppDomain — so a
    // probe path alone would leave them unloaded forever.
    private static void LoadUserLibs() {
        string directory = MelonEnvironment.UserLibsDirectory;
        if(!Directory.Exists(directory)) return;

        HashSet<string> loaded = new(
            AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name),
            StringComparer.OrdinalIgnoreCase);

        foreach(string file in Directory.GetFiles(directory, "*.dll").OrderBy(f => f, StringComparer.OrdinalIgnoreCase)) {
            string name;
            try { name = AssemblyName.GetAssemblyName(file).Name; } catch { continue; } // native library
            if(!loaded.Add(name)) continue; // never load a second copy of something already present
            MelonAssembly.LoadMelonAssembly(file, false);
        }
    }

    private static readonly HashSet<string> LibraryFolders =
        new(StringComparer.OrdinalIgnoreCase) { "UserLibs", "Dependencies", "Libs", "lib" };

    private static void LateStart() {
        started = true;

        // UnityModManager has finished its own load pass by now, so the mod list
        // can be rewritten: MelonCompat's own row goes away and the MelonLoader
        // mods take its place.
        UmmModList.Publish(entry, MelonBase.Snapshot);
        RefreshViews();

        MelonEvents.OnApplicationLateStart.Invoke();
        MelonBase.ExecuteAll(m => {
            m.OnLateInitializeMelon();
            m.OnApplicationLateStart();
        });
        MelonBase.Execute(p => p.OnApplicationStarted(), plugins);
    }

    private static void OnUpdate() {
        // MelonLoader's late start is "one frame after everything is up", which is
        // the first Update this host ever sees.
        if(!started) LateStart();

        // A scene reports as loaded before its objects have run Awake and Start.
        // OnSceneWasInitialized is the frame after, which is what mods expect.
        if(pendingSceneBuildIndex >= 0 || pendingSceneName != null) {
            int buildIndex = pendingSceneBuildIndex;
            string sceneName = pendingSceneName;
            pendingSceneBuildIndex = -1;
            pendingSceneName = null;
            MelonEvents.OnSceneWasInitialized.Invoke(buildIndex, sceneName);
            MelonBase.Execute(m => {
                m.OnSceneWasInitialized(buildIndex, sceneName);
                m.OnLevelWasInitialized(buildIndex);
            }, mods);
        }

        MelonEvents.OnUpdate.Invoke();
        MelonBase.Execute(m => m.OnUpdate(), active);
    }

    private static void OnFixedUpdate() {
        MelonEvents.OnFixedUpdate.Invoke();
        MelonBase.Execute(m => m.OnFixedUpdate(), active);
    }

    private static void OnLateUpdate() {
        MelonEvents.OnLateUpdate.Invoke();
        MelonBase.Execute(m => m.OnLateUpdate(), active);
    }

    private static void OnGui() {
        MelonEvents.OnGUI.Invoke();
        MelonBase.Execute(m => m.OnGUI(), active);
    }

    private static void OnApplicationQuit() {
        MelonPreferences.Save();
        MelonEvents.OnApplicationQuit.Invoke();
        MelonBase.ExecuteAll(m => m.OnApplicationQuit());
        MelonEvents.OnApplicationDefiniteQuit.Invoke();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        pendingSceneBuildIndex = scene.buildIndex;
        pendingSceneName = scene.name;
        MelonEvents.OnSceneWasLoaded.Invoke(scene.buildIndex, scene.name);
        MelonBase.Execute(m => {
            m.OnSceneWasLoaded(scene.buildIndex, scene.name);
            m.OnLevelWasLoaded(scene.buildIndex);
        }, mods);
    }

    private static void OnSceneUnloaded(Scene scene) {
        MelonEvents.OnSceneWasUnloaded.Invoke(scene.buildIndex, scene.name);
        MelonBase.Execute(m => m.OnSceneWasUnloaded(scene.buildIndex, scene.name), mods);
    }

    private static bool OnUnload(UnityModManager.ModEntry modEntry) {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        UnityHost.OnUpdateCallback = null;
        UnityHost.OnFixedUpdateCallback = null;
        UnityHost.OnLateUpdateCallback = null;
        UnityHost.OnGuiCallback = null;
        UnityHost.OnQuitCallback = null;

        UmmModList.Changed -= RefreshViews;
        UmmModList.Withdraw();

        MelonPreferences.Save();
        foreach(MelonAssembly assembly in MelonAssembly.LoadedAssemblies.ToArray())
            assembly.UnregisterMelons("MelonCompat is unloading", true);

        UnityHost.Destroy();
        started = false;
        return true;
    }
}
