using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using MelonLoader.Utils;
using UnityEngine;

namespace MelonLoader;

// The loader's grab-bag of helpers. Mods lean on these more than they should, so
// the whole practical surface is here rather than the few obvious ones.
public static class MelonUtils {
    public static MelonPlatformAttribute.CompatiblePlatforms CurrentPlatform =>
        IsMac ? MelonPlatformAttribute.CompatiblePlatforms.MAC_X64
        : IsWindows ? (IntPtr.Size == 8 ? MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64 : MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X86)
        : IntPtr.Size == 8 ? MelonPlatformAttribute.CompatiblePlatforms.LINUX_X64
        : MelonPlatformAttribute.CompatiblePlatforms.LINUX_X86;

    // UnityModManager only exists in a managed domain, so this is always Mono.
    public static MelonPlatformDomainAttribute.CompatibleDomains CurrentDomain =>
        MelonPlatformDomainAttribute.CompatibleDomains.MONO;

    public static MelonGameAttribute CurrentGameAttribute => new(GameDeveloper, GameName);

    public static string GameDeveloper => Application.companyName;
    public static string GameName => Application.productName;
    public static string GameVersion => Application.version;
    public static string CurrentProcessName => SafeProcessName();

    public static string BaseDirectory => MelonEnvironment.MelonBaseDirectory;
    public static string GameDirectory => MelonEnvironment.GameRootDirectory;
    public static string MelonLoaderDirectory => MelonEnvironment.MelonLoaderDirectory;
    public static string UserDataDirectory => MelonEnvironment.UserDataDirectory;
    public static string UserLibsDirectory => MelonEnvironment.UserLibsDirectory;

    public static string HashCode => "MelonCompat";
    public static bool IsGameIl2Cpp => false;
    public static bool IsUnix => IsMac || GetPlatform == PlatformID.Unix;
    public static bool IsWindows => GetPlatform is PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE;
    public static bool IsMac => Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
    public static PlatformID GetPlatform => Environment.OSVersion.Platform;

    public static string GetUnityVersion() => Application.unityVersion;
    public static string GetApplicationPath() => MelonEnvironment.GameExecutablePath;
    public static string GetGameDataDirectory() => MelonEnvironment.UnityGameDataDirectory;
    public static string GetManagedDirectory() => MelonEnvironment.UnityGameManagedDirectory;

    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> =>
        value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

    public static string GetPathAncestor(string path, int parentLevel) {
        for(int i = 0; i < parentLevel && !string.IsNullOrEmpty(path); i++) path = Path.GetDirectoryName(path);
        return path;
    }

    public static string MakePlural(string str, int amount) => amount == 1 ? str : str + "s";

    public static bool IsTypeEqualToName(Type type1, string type2) => type1?.Name == type2;
    public static bool IsTypeEqualToFullName(Type type1, string type2) => type1?.FullName == type2;

    public static string ComputeSimpleSHA256Hash(string filePath) => ComputeHash(SHA256.Create(), filePath);
    public static string ComputeSimpleSHA512Hash(string filePath) => ComputeHash(SHA512.Create(), filePath);

    private static string ComputeHash(HashAlgorithm algorithm, string filePath) {
        using(algorithm)
        using(FileStream stream = File.OpenRead(filePath))
            return ToString(algorithm.ComputeHash(stream));
    }

    public static string ToString(byte[] data) => ToString(data, "X2", null);
    public static string ToString(byte[] data, string format) => ToString(data, format, null);
    public static string ToString(byte[] data, IFormatProvider provider) => ToString(data, "X2", provider);

    public static string ToString(byte[] data, string format, IFormatProvider provider) {
        if(data == null) return string.Empty;
        StringBuilder builder = new(data.Length * 2);
        foreach(byte value in data) builder.Append(value.ToString(format ?? "X2", provider));
        return builder.ToString();
    }

    public static T PullAttributeFromAssembly<T>(Assembly asm, bool inherit = false) where T : Attribute =>
        PullAttributesFromAssembly<T>(asm, inherit).FirstOrDefault();

    public static T[] PullAttributesFromAssembly<T>(Assembly asm, bool inherit = false) where T : Attribute {
        if(asm == null) return [];
        try {
            return (T[])Attribute.GetCustomAttributes(asm, typeof(T), inherit);
        } catch {
            return [];
        }
    }

    // A mod assembly that fails to load a few of its types is still usable; only
    // the broken types are dropped.
    public static IEnumerable<Type> GetValidTypes(Assembly asm) => GetValidTypes(asm, null);

    public static IEnumerable<Type> GetValidTypes(Assembly asm, LemonFunc<Type, bool> predicate) {
        Type[] types;
        try {
            types = asm.GetTypes();
        } catch(ReflectionTypeLoadException e) {
            types = e.Types.Where(t => t != null).ToArray();
        } catch {
            return [];
        }
        return predicate == null ? types : types.Where(t => predicate(t));
    }

    public static Type GetValidType(Assembly asm, string typeName) => GetValidType(asm, typeName, null);

    public static Type GetValidType(Assembly asm, string typeName, LemonFunc<Type, bool> predicate) =>
        GetValidTypes(asm, predicate).FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);

    public static void TryPatchAll(Harmony harmony, Assembly assembly) {
        foreach(Type type in GetValidTypes(assembly)) TryPatchAll(harmony, type);
    }

    public static void TryPatchAll(Harmony harmony, Type type) {
        if(harmony == null || type == null) return;
        try {
            harmony.CreateClassProcessor(type).Patch();
        } catch(Exception e) {
            MelonLogger.Error($"failed to patch {type.FullName}: {e}");
        }
    }

    public static HarmonyMethod ToNewHarmonyMethod(MethodInfo methodInfo) =>
        methodInfo == null ? null : new HarmonyMethod(methodInfo);

    public static void SetCurrentDomainBaseDirectory(string dirpath, AppDomain domain = null) {
        // .NET does not allow the base directory to be moved after the domain is
        // up. MelonLoader does it with reflection into private setup state, which
        // is not a fight worth having inside somebody else's process.
        MelonLogger.Warning($"SetCurrentDomainBaseDirectory('{dirpath}') is not supported under MelonCompat and was ignored.");
    }

    public static MelonBase GetMelonFromStackTrace() => GetMelonFromStackTrace(new StackTrace());

    // Which mod is calling? Walk the frames and match the declaring assembly
    // against the registry — the same trick MelonLoader uses to attribute logs.
    public static MelonBase GetMelonFromStackTrace(StackTrace st, bool allFrames = false) {
        if(st == null) return null;
        for(int i = 0; i < st.FrameCount; i++) {
            Assembly asm = st.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
            if(asm == null) continue;
            MelonBase melon = MelonBase.RegisteredMelons.FirstOrDefault(m => m.Assembly == asm);
            if(melon != null) return melon;
            if(!allFrames) continue;
        }
        return null;
    }

    private static string SafeProcessName() {
        try { return Process.GetCurrentProcess().ProcessName; } catch { return Application.productName; }
    }
}
