namespace MelonLoader;

// MelonLoader reads these from its own command line. Under UnityModManager none
// of those arguments exist, so mods that branch on them see the normal defaults.
public static class MelonLaunchOptions {
    public static class Core {
        public enum LoadModeEnum {
            NORMAL,
            DEV,
            BOTH,
        }

        public static LoadModeEnum LoadMode_Plugins => LoadModeEnum.NORMAL;
        public static LoadModeEnum LoadMode_Mods => LoadModeEnum.NORMAL;
        public static bool QuitFix => false;
        public static bool StartScreen => false;
        public static bool IsDebug => MelonDebug.IsEnabled;
    }

    public static class Console {
        public static bool ShouldSetTitle => false;
        public static bool AlwaysOnTop => false;
        public static bool HideWarnings => false;
        public static bool CleanUnityLogs => true;
    }

    public static class Logger {
        public static int MaxLogs => 10;
    }
}
