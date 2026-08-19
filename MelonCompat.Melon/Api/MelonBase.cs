using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader.Logging;
using MelonLoader.Support;

namespace MelonLoader;

// The base every mod derives from. The registry, the lifecycle callbacks and the
// per-melon Harmony instance all live here; the host in MelonLoader.Support is
// what actually drives them from Unity.
public abstract class MelonBase {
    public enum Incompatibility {
        MLVersion,
        MLBuild,
        Game,
        GameVersion,
        ProcessName,
        Domain,
        Platform,
    }

    private static readonly List<MelonBase> registry = [];

    public static readonly MelonEvent<MelonBase> OnMelonRegistered = new();
    public static readonly MelonEvent<MelonBase> OnMelonUnregistered = new();
    public static readonly MelonEvent<MelonBase> OnMelonInitializing = new();

    public readonly MelonEvent OnRegister = new(true);
    public readonly MelonEvent OnUnregister = new(true);

    private static MelonBase[] snapshot = [];

    public static ReadOnlyCollection<MelonBase> RegisteredMelons {
        get { lock(registry) return registry.AsReadOnly(); }
    }

    // The per-frame dispatch path reads this instead of copying the registry
    // sixty times a second. It is replaced wholesale on register and unregister,
    // so a callback that unregisters its own melon still finishes the current
    // pass over a stable array.
    internal static MelonBase[] Snapshot => snapshot;

    private static void RefreshSnapshot() {
        lock(registry) snapshot = registry.ToArray();
    }

    public MelonAssembly MelonAssembly { get; internal set; }
    public MelonInfoAttribute Info { get; internal set; }
    public MelonAdditionalCreditsAttribute AdditionalCredits { get; internal set; }
    public MelonProcessAttribute[] SupportedProcesses { get; internal set; } = [];
    public MelonGameAttribute[] Games { get; internal set; } = [];
    public MelonGameVersionAttribute[] SupportedGameVersions { get; internal set; } = [];
    public MelonOptionalDependenciesAttribute OptionalDependencies { get; internal set; }
    public MelonPlatformAttribute SupportedPlatforms { get; internal set; }
    public MelonPlatformDomainAttribute SupportedDomain { get; internal set; }
    public VerifyLoaderVersionAttribute SupportedMLVersion { get; internal set; }
    public VerifyLoaderBuildAttribute SupportedMLBuild { get; internal set; }
    public int Priority { get; internal set; }
    public ColorARGB ConsoleColor { get; internal set; } = MelonLogger.DefaultMelonColor;
    public ColorARGB AuthorConsoleColor { get; internal set; } = MelonLogger.DefaultTextColor;

    public string ID { get; internal set; }
    public bool Registered { get; private set; }
    public abstract string MelonTypeName { get; }

    public Harmony HarmonyInstance { get; internal set; }
    public MelonLogger.Instance LoggerInstance { get; internal set; }

    public Assembly Assembly => MelonAssembly?.Assembly;
    public string Location => MelonAssembly?.Location;
    public string Hash => MelonAssembly?.Hash;
    public bool HarmonyDontPatchAll => MelonAssembly?.HarmonyDontPatchAll ?? false;

    public virtual void OnPreSupportModule() { }
    public virtual void OnEarlyInitializeMelon() { }
    public virtual void OnInitializeMelon() { }
    public virtual void OnLateInitializeMelon() { }
    public virtual void OnDeinitializeMelon() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnGUI() { }
    public virtual void OnApplicationQuit() { }
    public virtual void OnPreferencesSaved() { }
    public virtual void OnPreferencesSaved(string filepath) { }
    public virtual void OnPreferencesLoaded() { }
    public virtual void OnPreferencesLoaded(string filepath) { }

    // Pre-0.3 lifecycle names. MelonLoader still calls them and mods still
    // override them, so they are routed alongside the modern ones.
    public virtual void OnApplicationStart() { }
    public virtual void OnApplicationLateStart() { }
    public virtual void OnModSettingsApplied() { }

    public bool Register() {
        if(Registered) return true;

        Incompatibility[] incompatibilities = FindIncompatiblitiesFromContext();
        if(incompatibilities.Length > 0) {
            PrintIncompatibilities(incompatibilities, this);
            return false;
        }

        LoggerInstance ??= new MelonLogger.Instance(Info?.Name ?? GetType().Name, ConsoleColor);
        HarmonyInstance ??= new Harmony(ID ?? GetType().FullName);

        lock(registry) {
            registry.Add(this);
            registry.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
        RefreshSnapshot();
        Registered = true;

        OnMelonInitializing.Invoke(this);
        try {
            OnEarlyInitializeMelon();
        } catch(Exception e) {
            LoggerInstance.Error($"OnEarlyInitializeMelon threw: {e}");
        }

        // MelonLoader applies a mod's [HarmonyPatch] classes for it; mods are
        // written expecting that and most never call PatchAll themselves.
        MelonEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority);

        OnRegister.Invoke();
        OnMelonRegistered.Invoke(this);
        return true;
    }

    // Patch every annotated class in the melon's own assembly, one at a time: a
    // single class that fails to patch must not take the mod's other patches with
    // it, which is why this is not one PatchAll call.
    internal void HarmonyInit() {
        if(HarmonyDontPatchAll || MelonAssembly?.Assembly == null) return;

        int patched = 0;
        List<string> failed = [];

        foreach(Type type in MelonUtils.GetValidTypes(MelonAssembly.Assembly)) {
            try {
                // Engine internal calls have no body for Harmony to wrap, so those
                // patch classes are taken over and applied by rewriting their call
                // sites instead. Asked first, because Harmony would only throw.
                if(Support.ExternPatcher.TryRegister(type, LoggerInstance)) continue;

                List<MethodInfo> methods = HarmonyInstance.CreateClassProcessor(type).Patch();
                if(methods != null) patched += methods.Count;
            } catch(Exception e) {
                failed.Add(type.FullName);
                LoggerInstance?.Error($"failed to apply the Harmony patches in {type.FullName}: {e}");
            }
        }

        if(patched > 0) LoggerInstance?.Msg($"applied {patched} Harmony {MelonUtils.MakePlural("patch", patched)}");
        if(failed.Count == 0) return;

        // A mod missing some of its patches is running outside the state it was
        // built and tested in, and the damage surfaces far from here — as a wrong
        // value, or a crash inside native code the mod handed bad sizes to. Say it
        // plainly instead of leaving it as one error among many.
        LoggerInstance?.Warning($"{failed.Count} patch {MelonUtils.MakePlural("class", failed.Count)} could not be applied: {string.Join(", ", failed.ToArray())}");
        LoggerInstance?.Warning("this mod is running only partially patched and may misbehave or crash.");
    }

    public void Unregister(string reason = null, bool silent = false) {
        if(!Registered) return;
        Registered = false;
        lock(registry) registry.Remove(this);
        RefreshSnapshot();

        try {
            OnDeinitializeMelon();
        } catch(Exception e) {
            LoggerInstance?.Error($"OnDeinitializeMelon threw: {e}");
        }

        // A mod that is going away must take its patches with it, or the game
        // keeps running detours into an assembly nobody is driving any more.
        HarmonyBridge.UnpatchOwn(HarmonyInstance, LoggerInstance);

        if(!silent)
            MelonLogger.Warning($"{Info?.Name ?? GetType().Name} was unregistered{(string.IsNullOrEmpty(reason) ? "" : ": " + reason)}");

        OnUnregister.Invoke();
        OnMelonUnregistered.Invoke(this);
    }

    public Incompatibility[] FindIncompatiblitiesFromContext() =>
        FindIncompatiblities(MelonUtils.CurrentGameAttribute, MelonUtils.CurrentProcessName, MelonUtils.GameVersion, BuildInfo.Version, MelonUtils.HashCode);

    public Incompatibility[] FindIncompatiblities(MelonGameAttribute game, string processName, string gameVersion, string mlVersion, string mlBuildHashCode) =>
        FindIncompatiblities(game, processName, gameVersion, Semver.SemVersion.Parse(mlVersion), mlBuildHashCode);

    public Incompatibility[] FindIncompatiblities(MelonGameAttribute game, string processName, string gameVersion, Semver.SemVersion mlVersion, string mlBuildHashCode) {
        List<Incompatibility> found = [];

        if(Games.Length > 0 && game != null && !Games.Any(g => g.IsCompatible(game)))
            found.Add(Incompatibility.Game);

        if(SupportedGameVersions.Length > 0 && !string.IsNullOrEmpty(gameVersion)
           && !SupportedGameVersions.Any(v => v.Universal || v.Version == gameVersion))
            found.Add(Incompatibility.GameVersion);

        if(SupportedProcesses.Length > 0 && !SupportedProcesses.Any(p => p.IsCompatible(processName)))
            found.Add(Incompatibility.ProcessName);

        if(SupportedPlatforms != null && !SupportedPlatforms.IsCompatible(MelonUtils.CurrentPlatform))
            found.Add(Incompatibility.Platform);

        if(SupportedDomain != null && !SupportedDomain.IsCompatible(MelonUtils.CurrentDomain))
            found.Add(Incompatibility.Domain);

        // The loader version a mod asks for is MelonLoader's, and this is not
        // MelonLoader. Reporting a mismatch would reject perfectly good mods, so
        // the check is deliberately skipped and only noted.
        if(SupportedMLVersion != null && mlVersion != null && !SupportedMLVersion.IsCompatible(mlVersion))
            MelonLogger.Warning($"{Info?.Name ?? GetType().Name} expects MelonLoader {SupportedMLVersion.SemVer}; MelonCompat reports {mlVersion} and is loading it anyway.");

        return found.ToArray();
    }

    public static void PrintIncompatibilities(Incompatibility[] incompatibilities, MelonBase melon) {
        if(incompatibilities == null || incompatibilities.Length == 0) return;
        MelonLogger.Warning($"{melon?.Info?.Name ?? "a mod"} is not compatible with this game: {string.Join(", ", incompatibilities.Select(i => i.ToString()).ToArray())}");
    }

    public static MelonBase FindMelon(string melonName, string melonAuthor) {
        lock(registry)
            return registry.FirstOrDefault(m => m.Info?.Name == melonName && m.Info?.Author == melonAuthor);
    }

    public static void ExecuteAll(LemonAction<MelonBase> func, bool unregisterOnFail = false, string unregistrationReason = null) =>
        Execute(func, Snapshot, unregisterOnFail, unregistrationReason);

    // One mod throwing must not stop the ones after it in the list — that is the
    // whole reason this runs per-melon instead of over a plain foreach.
    public static void ExecuteList<T>(LemonAction<T> func, List<T> melons, bool unregisterOnFail = false, string unregistrationReason = null) where T : MelonBase =>
        Execute(func, melons?.ToArray(), unregisterOnFail, unregistrationReason);

    internal static void Execute<T>(LemonAction<T> func, T[] melons, bool unregisterOnFail = false, string unregistrationReason = null) where T : MelonBase {
        if(func == null || melons == null) return;
        foreach(T melon in melons) {
            if(melon == null || !melon.Registered) continue;
            try {
                func(melon);
            } catch(Exception e) {
                melon.LoggerInstance?.Error($"{e}");
                if(unregisterOnFail) melon.Unregister(unregistrationReason ?? "it threw an unhandled exception");
            }
        }
    }

    public static void SendMessageAll(string name, params object[] arguments) {
        foreach(MelonBase melon in RegisteredMelons) melon.SendMessage(name, arguments);
    }

    public object SendMessage(string name, params object[] arguments) {
        MethodInfo method = GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if(method == null) return null;
        try {
            return method.Invoke(this, arguments);
        } catch(Exception e) {
            LoggerInstance?.Error($"SendMessage({name}) threw: {e}");
            return null;
        }
    }

    public static T CreateWrapper<T>(string name, string author, string version, MelonGameAttribute[] games = null, MelonProcessAttribute[] processes = null, int priority = 0, ConsoleColor color = System.ConsoleColor.Gray) where T : MelonBase, new() {
        T melon = new() {
            Info = new MelonInfoAttribute(typeof(T), name, version, author),
            Games = games ?? [],
            SupportedProcesses = processes ?? [],
            Priority = priority,
            ConsoleColor = ColorARGB.FromConsoleColor(color),
            ID = $"{author}.{name}",
        };
        melon.LoggerInstance = new MelonLogger.Instance(name, melon.ConsoleColor);
        return melon;
    }

    public static void RegisterSorted<T>(IEnumerable<T> melons) where T : MelonBase {
        if(melons == null) return;
        foreach(T melon in melons.OrderBy(m => m.Priority)) melon.Register();
    }
}

// MelonMod.RegisteredMelons and MelonPlugin.RegisteredMelons are separate typed
// views over the same registry; mods use them to find each other.
public abstract class MelonTypeBase<T> : MelonBase where T : MelonTypeBase<T> {
    public new static ReadOnlyCollection<T> RegisteredMelons =>
        MelonBase.RegisteredMelons.OfType<T>().ToList().AsReadOnly();
}
