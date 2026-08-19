using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader.Support;

namespace MelonLoader.Resolver;

public class AssemblyResolveInfo {
    public string Name;
    public Assembly Assembly;
    public readonly Dictionary<Version, Assembly> Versions = [];
}

// Mods call this to point the loader at a folder holding their dependencies. The
// directories are forwarded to the bootstrap's resolver, which is the thing that
// actually answers assembly loads in this process.
public static class MelonAssemblyResolver {
    private static readonly Dictionary<string, AssemblyResolveInfo> known = new(StringComparer.OrdinalIgnoreCase);

    public delegate void OnAssemblyLoadHandler(Assembly assembly);
    public delegate void OnAssemblyResolveHandler(string name, Version version, ref Assembly assembly);

    public static event OnAssemblyLoadHandler OnAssemblyLoad;
    public static event OnAssemblyResolveHandler OnAssemblyResolve;

    public static void AddSearchDirectory(string path, int priority = 0) => ProbePaths.Add(path);

    public static void AddSearchDirectories(params string[] paths) {
        foreach(string path in paths ?? []) AddSearchDirectory(path);
    }

    public static void AddSearchDirectories(int priority, params string[] paths) => AddSearchDirectories(paths);

    public static void AddSearchDirectories(params LemonTuple<string, int>[] paths) {
        foreach(LemonTuple<string, int> path in paths ?? []) AddSearchDirectory(path.Item1, path.Item2);
    }

    // The bootstrap's resolver holds one flat list of directories, so removal is
    // not supported; a directory that was added stays searchable.
    public static void RemoveSearchDirectory(string path) =>
        MelonLogger.Warning($"MelonAssemblyResolver.RemoveSearchDirectory('{path}') is not supported under MelonCompat and was ignored.");

    public static AssemblyResolveInfo GetAssemblyResolveInfo(string name) {
        if(known.TryGetValue(name, out AssemblyResolveInfo info)) return info;
        return known[name] = new AssemblyResolveInfo { Name = name };
    }

    public static void LoadInfoFromAssembly(Assembly assembly) {
        if(assembly == null) return;
        AssemblyName name = assembly.GetName();
        AssemblyResolveInfo info = GetAssemblyResolveInfo(name.Name);
        info.Assembly = assembly;
        info.Versions[name.Version] = assembly;
        OnAssemblyLoad?.Invoke(assembly);
    }

    internal static Assembly Resolve(string name, Version version) {
        Assembly assembly = null;
        OnAssemblyResolve?.Invoke(name, version, ref assembly);
        return assembly;
    }
}
