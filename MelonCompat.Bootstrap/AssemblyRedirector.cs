using System.IO;
using System.Reflection;

namespace MelonCompat.Bootstrap;

// Mods on disk were compiled against the real loader: they carry assembly
// references to "MelonLoader" and to HarmonyX's "0Harmony". Neither is present
// under UnityModManager, so every load would fail at the first type resolution.
//
// This redirector answers both by simple name — MelonLoader resolves to the
// payload shipped next to this bootstrap, 0Harmony to whatever Harmony
// UnityModManager already has loaded — and probes the payload, mod and userlib
// directories for anything else a mod brought with it.
public static class AssemblyRedirector {
    private static readonly object Sync = new();
    private static readonly List<string> ProbePaths = [];
    private static readonly Dictionary<string, Assembly> Pinned = new(StringComparer.OrdinalIgnoreCase);
    private static bool installed;

    public static void Install(Action<string> warn) {
        lock(Sync) {
            if(installed) return;
            installed = true;
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) => Resolve(args.Name, warn);
        }
    }

    // Pinning is how the two identity-critical assemblies get answered: the
    // request arrives with a version this process does not have, and the only
    // correct reply is "use this instance regardless of version".
    public static void Pin(string simpleName, Assembly assembly) {
        if(assembly == null) return;
        lock(Sync) Pinned[simpleName] = assembly;
    }

    public static void AddProbePath(string directory) {
        if(string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) return;
        lock(Sync) {
            string full = Path.GetFullPath(directory);
            if(!ProbePaths.Contains(full)) ProbePaths.Add(full);
        }
    }

    // The already-loaded assembly wins over anything on disk: loading a second
    // copy of Harmony would give the game two independent patch registries.
    public static Assembly FindLoaded(string simpleName) {
        foreach(Assembly loaded in AppDomain.CurrentDomain.GetAssemblies())
            if(string.Equals(loaded.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase))
                return loaded;
        return null;
    }

    private static Assembly Resolve(string fullName, Action<string> warn) {
        string simpleName;
        try { simpleName = new AssemblyName(fullName).Name; } catch { return null; }

        lock(Sync) {
            if(Pinned.TryGetValue(simpleName, out Assembly pinned)) return pinned;
        }

        Assembly already = FindLoaded(simpleName);
        if(already != null) return already;

        string[] directories;
        lock(Sync) directories = ProbePaths.ToArray();
        foreach(string directory in directories) {
            string path = Path.Combine(directory, simpleName + ".dll");
            if(!File.Exists(path)) continue;
            try {
                return Assembly.LoadFrom(path);
            } catch(Exception e) {
                warn?.Invoke($"[MelonCompat] could not load {path}: {e.Message}");
            }
        }
        return null;
    }
}
