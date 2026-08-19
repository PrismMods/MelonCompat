using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader.Logging;
using MelonLoader.Support;

namespace MelonLoader;

// A mod that could not be brought up, kept so the failure is reportable instead
// of silently missing.
public class RottenMelon {
    public Type type;
    public string errorMessage;
    public Exception exception;

    public RottenMelon(Type type, string errorMessage, Exception exception = null) {
        this.type = type;
        this.errorMessage = errorMessage;
        this.exception = exception;
    }
}

public class ResolvedMelons {
    public MelonBase[] loadedMelons;
    public RottenMelon[] rottenMelons;

    public ResolvedMelons(MelonBase[] loadedMelons, RottenMelon[] rottenMelons) {
        this.loadedMelons = loadedMelons ?? [];
        this.rottenMelons = rottenMelons ?? [];
    }
}

// One loaded mod file and the melons inside it. This is where a DLL on disk turns
// into registered MelonMod instances: load the assembly (translating HarmonyX
// references on the way in), read its attributes, instantiate, register.
public class MelonAssembly {
    private static readonly List<MelonAssembly> loadedAssemblies = [];
    private static readonly List<LemonFunc<Assembly, ResolvedMelons>> customResolvers = [];

    public static readonly MelonEvent<Assembly> OnAssemblyResolving = new();
    public readonly MelonEvent OnUnregister = new(true);

    private readonly List<MelonBase> melons = [];
    private readonly List<RottenMelon> rotten = [];
    private bool melonsLoaded;

    public Assembly Assembly { get; }
    public string Location { get; }
    public string Hash { get; }
    public bool HarmonyDontPatchAll { get; }

    public ReadOnlyCollection<MelonBase> LoadedMelons => melons.AsReadOnly();
    public ReadOnlyCollection<RottenMelon> RottenMelons => rotten.AsReadOnly();

    public static ReadOnlyCollection<MelonAssembly> LoadedAssemblies {
        get { lock(loadedAssemblies) return loadedAssemblies.AsReadOnly(); }
    }

    public static event LemonFunc<Assembly, ResolvedMelons> CustomMelonResolvers {
        add { lock(customResolvers) customResolvers.Add(value); }
        remove { lock(customResolvers) customResolvers.Remove(value); }
    }

    private MelonAssembly(Assembly assembly, string location) {
        Assembly = assembly;
        Location = location ?? SafeLocation(assembly);
        Hash = ComputeHash(Location);
        HarmonyDontPatchAll = assembly.GetCustomAttribute<HarmonyDontPatchAllAttribute>() != null;
    }

    public static MelonAssembly LoadMelonAssembly(string path, bool loadMelons = true) {
        if(!File.Exists(path)) {
            MelonLogger.Error($"no mod file at {path}");
            return null;
        }
        try {
            // Load the translated form when the mod uses HarmonyX-only APIs;
            // otherwise this is a plain LoadFrom and Assembly.Location survives,
            // which some mods read to find their own data files.
            Assembly assembly = HarmonyBridge.LoadTranslated(path);
            return LoadMelonAssembly(path, assembly, loadMelons);
        } catch(Exception e) {
            MelonLogger.Error($"could not load {Path.GetFileName(path)}: {e}");
            return null;
        }
    }

    public static MelonAssembly LoadRawMelonAssembly(string path, byte[] assemblyData, byte[] symbolsData = null, bool loadMelons = true) {
        try {
            Assembly assembly = symbolsData == null ? Assembly.Load(assemblyData) : Assembly.Load(assemblyData, symbolsData);
            return LoadMelonAssembly(path, assembly, loadMelons);
        } catch(Exception e) {
            MelonLogger.Error($"could not load the raw assembly for {path}: {e}");
            return null;
        }
    }

    public static MelonAssembly LoadMelonAssembly(string path, Assembly assembly, bool loadMelons = true) {
        if(assembly == null) return null;

        lock(loadedAssemblies) {
            MelonAssembly existing = loadedAssemblies.FirstOrDefault(a => a.Assembly == assembly);
            if(existing != null) return existing;
        }

        MelonAssembly melonAssembly = new(assembly, path);
        lock(loadedAssemblies) loadedAssemblies.Add(melonAssembly);
        OnAssemblyResolving.Invoke(assembly);

        if(loadMelons) melonAssembly.LoadMelons();
        return melonAssembly;
    }

    public static MelonAssembly GetMelonAssemblyOfMember(MemberInfo member, object obj = null) {
        Assembly asm = member?.DeclaringType?.Assembly ?? obj?.GetType().Assembly;
        if(asm == null) return null;
        lock(loadedAssemblies) return loadedAssemblies.FirstOrDefault(a => a.Assembly == asm);
    }

    public void LoadMelons() {
        if(melonsLoaded) return;
        melonsLoaded = true;

        ResolvedMelons resolved = Resolve();
        foreach(RottenMelon rottenMelon in resolved.rottenMelons) {
            rotten.Add(rottenMelon);
            string subject = rottenMelon.type?.FullName ?? Path.GetFileName(Location);
            // A DLL with no melon type in it is almost always a library a mod
            // shipped alongside itself, not a broken mod. Saying so at error level
            // would train people to ignore the log.
            if(rottenMelon.type == null) MelonLogger.Msg($"{subject}: {rottenMelon.errorMessage}");
            else MelonLogger.Error($"{subject}: {rottenMelon.errorMessage}");
            if(rottenMelon.exception != null) MelonLogger.Error(rottenMelon.exception.ToString());
        }

        foreach(MelonBase melon in resolved.loadedMelons.OrderBy(m => m.Priority)) {
            melon.MelonAssembly = this;
            if(!melon.Register()) continue;
            melons.Add(melon);
        }
    }

    public void UnregisterMelons(string reason = null, bool silent = false) {
        foreach(MelonBase melon in melons.ToArray()) melon.Unregister(reason, silent);
        melons.Clear();
        OnUnregister.Invoke();
        lock(loadedAssemblies) loadedAssemblies.Remove(this);
    }

    private ResolvedMelons Resolve() {
        LemonFunc<Assembly, ResolvedMelons>[] resolvers;
        lock(customResolvers) resolvers = customResolvers.ToArray();
        foreach(LemonFunc<Assembly, ResolvedMelons> resolver in resolvers) {
            ResolvedMelons custom = resolver(Assembly);
            if(custom != null && (custom.loadedMelons.Length > 0 || custom.rottenMelons.Length > 0)) return custom;
        }

        MelonInfoAttribute info = Assembly.GetCustomAttribute<MelonInfoAttribute>();
        if(info == null)
            return new ResolvedMelons([], [new RottenMelon(null, "it has no MelonInfo attribute, so it is not a MelonLoader mod")]);

        if(info.SystemType == null || !info.SystemType.IsSubclassOf(typeof(MelonBase)))
            return new ResolvedMelons([], [new RottenMelon(info.SystemType, "the type named in its MelonInfo attribute does not derive from MelonMod or MelonPlugin")]);

        MelonBase melon;
        try {
            melon = (MelonBase)Activator.CreateInstance(info.SystemType, true);
        } catch(Exception e) {
            return new ResolvedMelons([], [new RottenMelon(info.SystemType, "it could not be constructed", e)]);
        }

        ApplyAttributes(melon, info);
        return new ResolvedMelons([melon], []);
    }

    private void ApplyAttributes(MelonBase melon, MelonInfoAttribute info) {
        melon.Info = info;
        melon.MelonAssembly = this;
        melon.AdditionalCredits = Assembly.GetCustomAttribute<MelonAdditionalCreditsAttribute>();
        melon.OptionalDependencies = Assembly.GetCustomAttribute<MelonOptionalDependenciesAttribute>();
        melon.SupportedPlatforms = Assembly.GetCustomAttribute<MelonPlatformAttribute>();
        melon.SupportedDomain = Assembly.GetCustomAttribute<MelonPlatformDomainAttribute>();
        melon.SupportedMLVersion = Assembly.GetCustomAttribute<VerifyLoaderVersionAttribute>();
        melon.SupportedMLBuild = Assembly.GetCustomAttribute<VerifyLoaderBuildAttribute>();
        melon.SupportedProcesses = MelonUtils.PullAttributesFromAssembly<MelonProcessAttribute>(Assembly);
        melon.Games = MelonUtils.PullAttributesFromAssembly<MelonGameAttribute>(Assembly);
        melon.SupportedGameVersions = MelonUtils.PullAttributesFromAssembly<MelonGameVersionAttribute>(Assembly);
        melon.Priority = Assembly.GetCustomAttribute<MelonPriorityAttribute>()?.Priority ?? 0;

        MelonColorAttribute color = Assembly.GetCustomAttribute<MelonColorAttribute>();
        if(color != null) melon.ConsoleColor = color.Color;
        MelonAuthorColorAttribute authorColor = Assembly.GetCustomAttribute<MelonAuthorColorAttribute>();
        if(authorColor != null) melon.AuthorConsoleColor = authorColor.Color;

        melon.ID = Assembly.GetCustomAttribute<MelonIDAttribute>()?.ID ?? $"{info.Author}.{info.Name}";
        melon.LoggerInstance = new MelonLogger.Instance(info.Name, melon.ConsoleColor);
    }

    private static string SafeLocation(Assembly assembly) {
        try { return assembly.Location; } catch { return null; }
    }

    private static string ComputeHash(string location) {
        try {
            return string.IsNullOrEmpty(location) || !File.Exists(location) ? null : MelonUtils.ComputeSimpleSHA256Hash(location);
        } catch {
            return null;
        }
    }
}
