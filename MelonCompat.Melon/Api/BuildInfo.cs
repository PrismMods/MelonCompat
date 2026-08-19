namespace MelonLoader.Properties {
    // Mods read these to branch on loader version. The version reported is the
    // MelonLoader release whose API surface MelonCompat mirrors, not MelonCompat's
    // own — a mod comparing against it is asking "what API can I use".
    //
    // The const/property split is not cosmetic: mods compiled against the real
    // loader emit a property call for Version and inline the consts, so Version
    // has to stay a property or those mods fail to bind.
    public static class BuildInfo {
        public const string Name = "MelonLoader";
        public const string Description = "MelonLoader API compatibility layer for UnityModManager";
        public const string Author = "MelonCompat";
        public const string Company = null;

        public static string Version => "0.7.3";
        public static Semver.SemVersion VersionNumber => Semver.SemVersion.Parse(Version);
    }
}

namespace MelonLoader {
    // The pre-0.6 location. Every member is a property here, matching the real
    // loader's forwarders.
    public static class BuildInfo {
        public static string Name => Properties.BuildInfo.Name;
        public static string Description => Properties.BuildInfo.Description;
        public static string Author => Properties.BuildInfo.Author;
        public static string Company => Properties.BuildInfo.Company;
        public static string Version => Properties.BuildInfo.Version;
        public static Semver.SemVersion VersionNumber => Properties.BuildInfo.VersionNumber;
    }
}
