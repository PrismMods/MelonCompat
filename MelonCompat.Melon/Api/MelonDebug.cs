namespace MelonLoader;

// Debug output is on only when UnityModManager itself is in debug mode; there is
// no MelonLoader launch argument to read here.
public static class MelonDebug {
    public static bool IsEnabled { get; set; }

    public static event Action<ConsoleColor, string> MsgCallbackHandler;
    public static event Action<string> ErrorCallbackHandler;

    public static void Msg(object obj) => Msg(obj?.ToString());
    public static void Msg(string txt) {
        if(!IsEnabled) return;
        MelonLogger.Msg("[DEBUG] " + txt);
        MsgCallbackHandler?.Invoke(ConsoleColor.Gray, txt);
    }

    public static void Msg(string txt, params object[] args) => Msg(args == null || args.Length == 0 ? txt : string.Format(txt, args));

    public static void Error(string txt) {
        if(!IsEnabled) return;
        MelonLogger.Error("[DEBUG] " + txt);
        ErrorCallbackHandler?.Invoke(txt);
    }
}
