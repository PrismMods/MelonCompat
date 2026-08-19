namespace MelonLoader.Support;

// The one place mod output leaves this assembly. The host points it at
// UnityModManager's per-mod logger once it has a ModEntry.
//
// The Unity fallback is deliberately kept behind a method call rather than in a
// field initialiser: touching UnityEngine.Debug at type-initialisation time would
// drag the engine in before anything has asked it to, which makes this assembly
// unloadable anywhere but inside a running game.
public static class LogSink {
    private static Action<string> msg;
    private static Action<string> warning;
    private static Action<string> error;

    public static void Bind(Action<string> onMsg, Action<string> onWarning, Action<string> onError) {
        if(onMsg != null) msg = onMsg;
        if(onWarning != null) warning = onWarning;
        if(onError != null) error = onError;
    }

    public static void Msg(string text) {
        if(msg != null) Safe(msg, text);
        else UnityMsg(text);
    }

    public static void Warning(string text) {
        if(warning != null) Safe(warning, text);
        else UnityWarning(text);
    }

    public static void Error(string text) {
        if(error != null) Safe(error, text);
        else UnityError(text);
    }

    private static void UnityMsg(string text) => Safe(t => UnityEngine.Debug.Log(t), text);
    private static void UnityWarning(string text) => Safe(t => UnityEngine.Debug.LogWarning(t), text);
    private static void UnityError(string text) => Safe(t => UnityEngine.Debug.LogError(t), text);

    // Logging must never be the thing that takes the game down.
    private static void Safe(Action<string> sink, string text) {
        try { sink?.Invoke(text ?? string.Empty); } catch { }
    }
}
