using System.IO;
using System.Reflection;
using UnityModManagerNet;

namespace MelonCompat.Bootstrap;

// The UnityModManager entry point. It stays deliberately thin: install the
// assembly redirector, hand the payload the ModEntry, and get out of the way.
// The split matters because the payload *is* the MelonLoader shim — it cannot be
// referenced directly without the CLR resolving "MelonLoader" before the
// redirector that answers for it is in place.
public static class LoaderUmm {
    private const string PayloadDirectory = "Runtime";
    private const string PayloadFileName = "MelonLoader.dll";
    private const string HostTypeName = "MelonLoader.Support.MelonCompatHost";
    private const string HostMethodName = "Load";

    public static bool Load(UnityModManager.ModEntry modEntry) {
        UnityModManager.ModEntry.ModLogger log = modEntry.Logger;
        try {
            // Running under the real MelonLoader means MelonLoader.dll is already
            // in the process. Loading a second one would double-register every
            // mod and give each one two static states, so stop instead.
            Assembly existing = AssemblyRedirector.FindLoaded("MelonLoader");
            if(existing != null && !IsOurPayload(existing, modEntry.Path)) {
                log.Error("MelonLoader itself is running in this process — MelonCompat is redundant here.");
                log.Error("Put your mods in the game's Mods folder instead, or launch without MelonLoader.");
                return false;
            }

            string payloadPath = Path.Combine(modEntry.Path, PayloadDirectory, PayloadFileName);
            if(!File.Exists(payloadPath)) {
                log.Error($"the payload is missing: {payloadPath}");
                return false;
            }

            AssemblyRedirector.Install(log.Warning);
            AssemblyRedirector.AddProbePath(Path.Combine(modEntry.Path, PayloadDirectory));

            Assembly harmony = AssemblyRedirector.FindLoaded("0Harmony");
            if(harmony == null) {
                log.Error("UnityModManager's Harmony is not loaded — mods cannot be patched.");
                return false;
            }
            AssemblyRedirector.Pin("0Harmony", harmony);

            Assembly payload = Assembly.LoadFrom(Path.GetFullPath(payloadPath));
            AssemblyRedirector.Pin("MelonLoader", payload);

            return Invoke(payload, modEntry) is not false;
        } catch(Exception e) {
            log.Error($"the MelonCompat bootstrap failed: {e}");
            return false;
        }
    }

    private static bool IsOurPayload(Assembly assembly, string modPath) {
        string location;
        try { location = assembly.Location; } catch { return false; }
        return !string.IsNullOrEmpty(location)
            && Path.GetFullPath(location).StartsWith(Path.GetFullPath(modPath), StringComparison.OrdinalIgnoreCase);
    }

    private static object Invoke(Assembly payload, UnityModManager.ModEntry modEntry) {
        Type host = payload.GetType(HostTypeName, throwOnError: true);
        MethodInfo method = host.GetMethod(
            HostMethodName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            [typeof(UnityModManager.ModEntry), typeof(Action<string>)],
            null) ?? throw new MissingMethodException(HostTypeName, HostMethodName);
        Action<string> addProbePath = AssemblyRedirector.AddProbePath;
        try {
            return method.Invoke(null, [modEntry, addProbePath]);
        } catch(TargetInvocationException e) when(e.InnerException != null) {
            throw e.InnerException;
        }
    }
}
