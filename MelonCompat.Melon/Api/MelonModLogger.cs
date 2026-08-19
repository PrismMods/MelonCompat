namespace MelonLoader;

// The pre-0.3 logger name. Everything forwards to MelonLogger.
public static class MelonModLogger {
    public static void Log(string txt) => MelonLogger.Msg(txt);
    public static void Log(string txt, params object[] args) => MelonLogger.Msg(txt, args);
    public static void Log(ConsoleColor color, string txt) => MelonLogger.Msg(color, txt);
    public static void Log(ConsoleColor color, string txt, params object[] args) => MelonLogger.Msg(color, txt, args);
    public static void LogWarning(string txt) => MelonLogger.Warning(txt);
    public static void LogWarning(string txt, params object[] args) => MelonLogger.Warning(txt, args);
    public static void LogError(string txt) => MelonLogger.Error(txt);
    public static void LogError(string txt, params object[] args) => MelonLogger.Error(txt, args);
}
