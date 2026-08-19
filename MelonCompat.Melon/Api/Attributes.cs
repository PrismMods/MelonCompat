using System.Linq;

namespace MelonLoader;

// Every attribute a mod can carry. They are read, not merely tolerated: MelonInfo
// supplies the identity shown in the log, MelonPriority decides load order, and
// the compatibility attributes decide whether a mod is loaded at all.

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonInfoAttribute : Attribute {
    public Type SystemType { get; }
    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string DownloadLink { get; }
    public Semver.SemVersion SemanticVersion => Semver.SemVersion.Parse(Version);

    public MelonInfoAttribute(Type type, string name, string version, string author, string downloadLink = null) {
        SystemType = type;
        Name = name ?? "UNKNOWN";
        Version = string.IsNullOrEmpty(version) ? "1.0.0" : version;
        Author = author;
        DownloadLink = downloadLink;
    }

    public MelonInfoAttribute(Type type, string name, int versionMajor, int versionMinor, int versionRevision, string author, string downloadLink = null)
        : this(type, name, $"{versionMajor}.{versionMinor}.{versionRevision}", author, downloadLink) { }

    public MelonInfoAttribute(Type type, string name, int versionMajor, int versionMinor, int versionRevision, string versionIdentifier, string author, string downloadLink = null)
        : this(type, name, $"{versionMajor}.{versionMinor}.{versionRevision}" + (string.IsNullOrEmpty(versionIdentifier) ? "" : "-" + versionIdentifier), author, downloadLink) { }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class MelonGameAttribute : Attribute {
    public string Developer { get; }
    public string Name { get; }
    public bool Universal => Developer == null || Name == null;

    public MelonGameAttribute(string developer = null, string name = null) {
        Developer = developer;
        Name = name;
    }

    public bool IsCompatible(string developer, string gameName) =>
        Universal || (Developer == developer && Name == gameName);

    public bool IsCompatible(MelonGameAttribute att) =>
        att == null || Universal || att.Universal || IsCompatible(att.Developer, att.Name);

    public bool IsCompatibleBecauseUniversal(MelonGameAttribute att) =>
        Universal || (att?.Universal ?? false);
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class MelonGameVersionAttribute : Attribute {
    public string Version { get; }
    public bool Universal => string.IsNullOrEmpty(Version);
    public MelonGameVersionAttribute(string version = null) => Version = version;
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonPriorityAttribute : Attribute {
    public int Priority;
    public MelonPriorityAttribute(int priority = 0) => Priority = priority;
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonColorAttribute : Attribute {
    public Logging.ColorARGB Color { get; }
    public ConsoleColor DrawingColor { get; }

    public MelonColorAttribute() {
        Color = MelonLogger.DefaultMelonColor;
        DrawingColor = ConsoleColor.Gray;
    }

    public MelonColorAttribute(ConsoleColor color) {
        DrawingColor = color;
        Color = Logging.ColorARGB.FromConsoleColor(color);
    }

    public MelonColorAttribute(byte a, byte r, byte g, byte b) {
        Color = Logging.ColorARGB.FromArgb(a, r, g, b);
        DrawingColor = ConsoleColor.Gray;
    }

    public MelonColorAttribute(byte r, byte g, byte b) : this((byte)255, r, g, b) { }
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonAuthorColorAttribute : Attribute {
    public Logging.ColorARGB Color { get; }
    public ConsoleColor DrawingColor { get; }

    public MelonAuthorColorAttribute() {
        Color = MelonLogger.DefaultTextColor;
        DrawingColor = ConsoleColor.Gray;
    }

    public MelonAuthorColorAttribute(ConsoleColor color) {
        DrawingColor = color;
        Color = Logging.ColorARGB.FromConsoleColor(color);
    }

    public MelonAuthorColorAttribute(byte a, byte r, byte g, byte b) {
        Color = Logging.ColorARGB.FromArgb(a, r, g, b);
        DrawingColor = ConsoleColor.Gray;
    }

    public MelonAuthorColorAttribute(byte r, byte g, byte b) : this((byte)255, r, g, b) { }
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonAdditionalCreditsAttribute : Attribute {
    public string[] Credits { get; }
    public MelonAdditionalCreditsAttribute(params string[] credits) => Credits = credits ?? [];
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonOptionalDependenciesAttribute : Attribute {
    public string[] AssemblyNames { get; }
    public MelonOptionalDependenciesAttribute(params string[] assemblyNames) => AssemblyNames = assemblyNames ?? [];
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonAdditionalDependenciesAttribute : Attribute {
    public string[] AssemblyNames { get; }
    public MelonAdditionalDependenciesAttribute(params string[] assemblyNames) => AssemblyNames = assemblyNames ?? [];
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonIncompatibleAssembliesAttribute : Attribute {
    public string[] AssemblyNames { get; }
    public MelonIncompatibleAssembliesAttribute(params string[] assemblyNames) => AssemblyNames = assemblyNames ?? [];
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class MelonProcessAttribute : Attribute {
    public string EXE_Name { get; }
    public bool Universal => string.IsNullOrEmpty(EXE_Name);

    public MelonProcessAttribute(string exeName = null) =>
        EXE_Name = string.IsNullOrEmpty(exeName) ? null : exeName.Replace(".exe", "");

    public bool IsCompatible(string processName) =>
        Universal || string.IsNullOrEmpty(processName) || EXE_Name == processName.Replace(".exe", "");
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonIDAttribute : Attribute {
    public string ID { get; }
    public MelonIDAttribute(string id = null) => ID = id;
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonPlatformAttribute : Attribute {
    public enum CompatiblePlatforms {
        UNIVERSAL,
        WINDOWS_X86,
        WINDOWS_X64,
        LINUX_X86,
        LINUX_X64,
        MAC_X64,
    }

    public CompatiblePlatforms[] Platforms { get; }
    public MelonPlatformAttribute(params CompatiblePlatforms[] platforms) => Platforms = platforms ?? [];
    public bool IsCompatible(CompatiblePlatforms platform) =>
        Platforms.Length == 0 || Platforms.Contains(CompatiblePlatforms.UNIVERSAL) || Platforms.Contains(platform);
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonPlatformDomainAttribute : Attribute {
    public enum CompatibleDomains {
        UNIVERSAL,
        MONO,
        IL2CPP,
    }

    public CompatibleDomains Domain { get; }
    public MelonPlatformDomainAttribute(CompatibleDomains domain = CompatibleDomains.UNIVERSAL) => Domain = domain;
    public bool IsCompatible(CompatibleDomains domain) =>
        Domain == CompatibleDomains.UNIVERSAL || domain == CompatibleDomains.UNIVERSAL || Domain == domain;
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class VerifyLoaderVersionAttribute : Attribute {
    public Semver.SemVersion SemVer { get; }
    public bool IsMinimum { get; }
    public int Major => SemVer?.Major ?? 0;
    public int Minor => SemVer?.Minor ?? 0;
    public int Patch => SemVer?.Patch ?? 0;
    public string Prerelease => SemVer?.Prerelease;

    public VerifyLoaderVersionAttribute(string version, bool isMinimum = false) {
        SemVer = Semver.SemVersion.Parse(version);
        IsMinimum = isMinimum;
    }

    public VerifyLoaderVersionAttribute(int major, int minor, int patch, bool isMinimum = false)
        : this($"{major}.{minor}.{patch}", isMinimum) { }

    public VerifyLoaderVersionAttribute(int major, int minor, int patch) : this(major, minor, patch, false) { }

    public bool IsCompatible(Semver.SemVersion version) =>
        version == null || SemVer == null || (IsMinimum ? version.CompareTo(SemVer) >= 0 : version.CompareTo(SemVer) == 0);
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class VerifyLoaderBuildAttribute : Attribute {
    public string HashCode { get; }
    public VerifyLoaderBuildAttribute(string hashCode) => HashCode = hashCode;
}

// A mod that opts out of automatic PatchAll wants to drive Harmony itself.
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
public class HarmonyDontPatchAllAttribute : Attribute { }

// Il2Cpp-only registration. This game is Mono, so the attributes exist purely so
// assemblies carrying them still load.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RegisterTypeInIl2CppAttribute : Attribute {
    public bool LogSuccess { get; }
    public Type BaseType { get; }
    public RegisterTypeInIl2CppAttribute(bool logSuccess = true) => LogSuccess = logSuccess;
    public RegisterTypeInIl2CppAttribute(Type baseType, bool logSuccess = true) {
        BaseType = baseType;
        LogSuccess = logSuccess;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RegisterTypeInIl2CppWithInterfacesAttribute : Attribute {
    public Type[] Interfaces { get; }
    public bool LogSuccess { get; }
    public RegisterTypeInIl2CppWithInterfacesAttribute(params Type[] interfaces) : this(true, interfaces) { }
    public RegisterTypeInIl2CppWithInterfacesAttribute(bool logSuccess, params Type[] interfaces) {
        LogSuccess = logSuccess;
        Interfaces = interfaces ?? [];
    }
}

// Pre-0.3 names. Old mods still carry them and MelonLoader still reads them.
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonModInfoAttribute : MelonInfoAttribute {
    public MelonModInfoAttribute(Type type, string name, string version, string author, string downloadLink = null)
        : base(type, name, version, author, downloadLink) { }
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class MelonPluginInfoAttribute : MelonInfoAttribute {
    public MelonPluginInfoAttribute(Type type, string name, string version, string author, string downloadLink = null)
        : base(type, name, version, author, downloadLink) { }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class MelonModGameAttribute : MelonGameAttribute {
    public MelonModGameAttribute(string developer = null, string name = null) : base(developer, name) { }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class MelonPluginGameAttribute : MelonGameAttribute {
    public MelonPluginGameAttribute(string developer = null, string name = null) : base(developer, name) { }
}
