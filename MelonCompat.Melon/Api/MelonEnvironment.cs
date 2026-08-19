using System.IO;
using UnityEngine;

namespace MelonLoader.Utils;

// MelonLoader lays its directories out relative to the game root. Under UMM
// nothing establishes that root, so it is derived from Application.dataPath and
// then confirmed by looking for the folders that should be sitting in it.
public static class MelonEnvironment {
    private const string ModsFolderName = "MelonMods";
    private static string gameRoot;

    public static string GameRootDirectory => gameRoot ??= ResolveGameRoot();

    // UnityModManager knows exactly where the game is — it found the Mods folder
    // there. Taking the root from it beats deriving one from Application.dataPath,
    // whose shape differs per platform and, on a macOS bundle, is four levels deep
    // inside the install rather than one.
    public static void SetGameRootDirectory(string path) {
        if(string.IsNullOrEmpty(path)) return;
        gameRoot = Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }
    public static string ModsDirectory => Path.Combine(GameRootDirectory, ModsFolderName);
    public static string PluginsDirectory => Path.Combine(GameRootDirectory, "Plugins");
    public static string UserDataDirectory => Path.Combine(GameRootDirectory, "UserData");
    public static string UserLibsDirectory => Path.Combine(GameRootDirectory, "UserLibs");
    public static string MelonBaseDirectory => GameRootDirectory;
    public static string MelonLoaderDirectory => Path.Combine(GameRootDirectory, "MelonLoader");
    public static string MelonLoaderLogsDirectory => Path.Combine(MelonLoaderDirectory, "Logs");
    public static string DependenciesDirectory => Path.Combine(MelonLoaderDirectory, "Dependencies");
    public static string SupportModuleDirectory => Path.Combine(DependenciesDirectory, "SupportModules");
    public static string CompatibilityLayerDirectory => Path.Combine(DependenciesDirectory, "CompatibilityLayers");
    public static string Il2CppAssemblyGeneratorDirectory => Path.Combine(DependenciesDirectory, "Il2CppAssemblyGenerator");
    public static string Il2CppAssembliesDirectory => Path.Combine(MelonLoaderDirectory, "Il2CppAssemblies");
    public static string OurRuntimeDirectory => Path.GetDirectoryName(typeof(MelonEnvironment).Assembly.Location) ?? GameRootDirectory;
    public static string MelonManagedDirectory => OurRuntimeDirectory;

    public static string UnityGameDataDirectory => Application.dataPath;
    public static string UnityGameManagedDirectory => Path.Combine(Application.dataPath, "Managed");
    public static string Il2CppDataDirectory => Path.Combine(Application.dataPath, "il2cpp_data");

    public static string GameExecutablePath => Path.Combine(GameRootDirectory, GameExecutableName);
    public static string GameExecutableName => Application.productName;
    public static string UnityPlayerPath => GameExecutablePath;

    // ADOFAI ships as Mono, and this whole layer only makes sense on Mono anyway
    // — an Il2Cpp game has no managed UnityModManager to host it.
    public static bool IsMonoRuntime => true;
    public static bool IsDotnetRuntime => false;

    // dataPath is <root>/<Game>_Data on Windows and Linux, but
    // <root>/<Game>.app/Contents/Resources/Data on macOS. Both shapes are
    // recognisable from the path itself, so the root is derived rather than
    // guessed by probing upward — probing would silently sail past the real root
    // on a fresh install where none of the MelonLoader folders exist yet.
    private static string ResolveGameRoot() {
        // A trailing separator would silently cost one level of the climb.
        string data = Path.GetFullPath(Application.dataPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        bool macBundle = string.Equals(Path.GetFileName(data), "Data", StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path.GetFileName(Path.GetDirectoryName(data)), "Resources", StringComparison.OrdinalIgnoreCase);
        string root = macBundle ? Ascend(data, 4) : Path.GetDirectoryName(data);
        return string.IsNullOrEmpty(root) ? data : root;
    }

    private static string Ascend(string path, int levels) {
        for(int i = 0; i < levels && !string.IsNullOrEmpty(path); i++) path = Path.GetDirectoryName(path);
        return path;
    }
}
