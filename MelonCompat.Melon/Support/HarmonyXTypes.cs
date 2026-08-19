using System.Reflection;

// HarmonyX types that pardeike's Harmony does not define at all. The translator
// repoints mod type references here, which is what lets an assembly that merely
// mentions them finish loading.
namespace HarmonyLib {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HarmonyILManipulator : HarmonyAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HarmonyWrapSafe : HarmonyAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HarmonyEmitIL : HarmonyAttribute {
        public string Path { get; }
        public HarmonyEmitIL(string path = null) => Path = path;
    }

    public static class HarmonyGlobalSettings {
        public static bool DisallowLegacyGlobalUnpatchAll { get; set; }
    }

    // HarmonyX uses this to describe calli signatures in transpilers. Nothing
    // consumes it here, but transpilers that build one still need the type.
    public class InlineSignature {
        public bool HasThis { get; set; }
        public bool ExplicitThis { get; set; }
        public System.Runtime.InteropServices.CallingConvention CallingConvention { get; set; } =
            System.Runtime.InteropServices.CallingConvention.Winapi;
        public List<object> Parameters { get; set; } = [];
        public object ReturnType { get; set; } = typeof(void);
    }

    public class MemberNotFoundException : Exception {
        public MemberNotFoundException(string message) : base(message) { }
    }

    public class InvalidHarmonyPatchArgumentException : Exception {
        public MethodBase Original { get; }
        public MethodInfo Patch { get; }

        public InvalidHarmonyPatchArgumentException(string message, MethodBase original = null, MethodInfo patch = null)
            : base(message) {
            Original = original;
            Patch = patch;
        }
    }
}

namespace HarmonyLib.Tools {
    // HarmonyX's diagnostic logger. Mods enable channels on it during development
    // and leave the calls in; routing it into the mod log keeps that useful.
    public static class Logger {
        [Flags]
        public enum LogChannel {
            None = 0,
            Info = 1,
            IL = 2,
            Warn = 4,
            Error = 8,
            Debug = 16,
            All = Info | IL | Warn | Error | Debug,
        }

        public class LogEventArgs : EventArgs {
            public LogChannel LogChannel { get; }
            public string Message { get; }

            public LogEventArgs() { }

            public LogEventArgs(LogChannel logChannel, string message) {
                LogChannel = logChannel;
                Message = message;
            }
        }

        public static LogChannel ChannelFilter { get; set; } = LogChannel.None;
        public static event EventHandler<LogEventArgs> MessageReceived;

        public static void Log(LogChannel channel, Func<string> message, bool applyChannelFilter = true) {
            if(message == null) return;
            if(applyChannelFilter && (ChannelFilter & channel) == 0) return;
            LogText(channel, message());
        }

        public static void LogText(LogChannel channel, string message) {
            if(string.IsNullOrEmpty(message)) return;
            MessageReceived?.Invoke(null, new LogEventArgs(channel, message));
            if(channel == LogChannel.Error) MelonLoader.MelonLogger.Error("[Harmony] " + message);
            else if(channel == LogChannel.Warn) MelonLoader.MelonLogger.Warning("[Harmony] " + message);
            else MelonLoader.MelonLogger.Msg("[Harmony] " + message);
        }
    }

    public static class HarmonyFileLog {
        public static bool Enabled { get; set; }
        public static string FileWriterPath { get; set; }
    }
}
