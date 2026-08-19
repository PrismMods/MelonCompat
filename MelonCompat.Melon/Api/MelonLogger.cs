using System.Linq;
using MelonLoader.Logging;
using MelonLoader.Support;

namespace MelonLoader;

// Everything a mod logs ends up in UnityModManager's log. MelonLoader's colour
// arguments are accepted and forwarded to the callback handlers, but not baked
// into the text: UMM writes a plain file and rich-text tags would show up raw.
public static class MelonLogger {
    public static readonly ColorARGB DefaultMelonColor = ColorARGB.Cyan;
    public static readonly ColorARGB DefaultTextColor = ColorARGB.LightGray;

    public static event Action<ColorARGB, ColorARGB, string, string> MsgDrawingCallbackHandler;
    public static event Action<ConsoleColor, ConsoleColor, string, string> MsgCallbackHandler;
    public static event Action<string, string> WarningCallbackHandler;
    public static event Action<string, string> ErrorCallbackHandler;

    public static void Msg(object obj) => Write(null, obj?.ToString());
    public static void Msg(string txt) => Write(null, txt);
    public static void Msg(string txt, params object[] args) => Write(null, Format(txt, args));
    public static void Msg(ConsoleColor txtColor, object obj) => Write(null, obj?.ToString(), txtColor);
    public static void Msg(ConsoleColor txtColor, string txt) => Write(null, txt, txtColor);
    public static void Msg(ConsoleColor txtColor, string txt, params object[] args) => Write(null, Format(txt, args), txtColor);
    public static void Msg(ColorARGB txtColor, object obj) => Write(null, obj?.ToString(), null, txtColor);
    public static void Msg(ColorARGB txtColor, string txt) => Write(null, txt, null, txtColor);
    public static void Msg(ColorARGB txtColor, string txt, params object[] args) => Write(null, Format(txt, args), null, txtColor);
    public static void MsgDirect(ColorARGB txtColor, string txt) => Write(null, txt, null, txtColor);

    // MsgPastel exists because MelonLoader renders 24-bit colour in a terminal.
    // There is no terminal here, so it is Msg.
    public static void MsgPastel(object obj) => Msg(obj);
    public static void MsgPastel(string txt) => Msg(txt);
    public static void MsgPastel(string txt, params object[] args) => Msg(txt, args);
    public static void MsgPastel(ConsoleColor txtColor, object obj) => Msg(txtColor, obj);
    public static void MsgPastel(ConsoleColor txtColor, string txt) => Msg(txtColor, txt);
    public static void MsgPastel(ConsoleColor txtColor, string txt, params object[] args) => Msg(txtColor, txt, args);
    public static void MsgPastel(ColorARGB txtColor, object obj) => Msg(txtColor, obj);
    public static void MsgPastel(ColorARGB txtColor, string txt) => Msg(txtColor, txt);
    public static void MsgPastel(ColorARGB txtColor, string txt, params object[] args) => Msg(txtColor, txt, args);

    public static void Log(object obj) => Msg(obj);
    public static void Log(string txt) => Msg(txt);
    public static void Log(string txt, params object[] args) => Msg(txt, args);
    public static void Log(ConsoleColor txtColor, object obj) => Msg(txtColor, obj);
    public static void Log(ConsoleColor txtColor, string txt) => Msg(txtColor, txt);
    public static void Log(ConsoleColor txtColor, string txt, params object[] args) => Msg(txtColor, txt, args);

    public static void Warning(object obj) => Warn(null, obj?.ToString());
    public static void Warning(string txt) => Warn(null, txt);
    public static void Warning(string txt, params object[] args) => Warn(null, Format(txt, args));
    public static void LogWarning(string txt) => Warning(txt);
    public static void LogWarning(string txt, params object[] args) => Warning(txt, args);

    public static void Error(object obj) => Err(null, obj?.ToString());
    public static void Error(string txt) => Err(null, txt);
    public static void Error(string txt, params object[] args) => Err(null, Format(txt, args));
    public static void Error(string txt, Exception ex) => Err(null, txt + "\n" + ex);
    public static void LogError(string txt) => Error(txt);
    public static void LogError(string txt, params object[] args) => Error(txt, args);

    public static void WriteSpacer() => Write(null, string.Empty);
    public static void WriteLine(int length = 30) => Write(null, new string('-', Math.Max(1, length)));
    public static void WriteLine(ColorARGB color, int length = 30) => Write(null, new string('-', Math.Max(1, length)), null, color);

    public static void BigError(string txt) => BigError(null, txt);

    public static void BigError(string section, string txt) {
        Err(section, new string('=', 50));
        foreach(string line in (txt ?? string.Empty).Split('\n')) Err(section, line);
        Err(section, new string('=', 50));
    }

    private static string Format(string txt, object[] args) {
        if(txt == null) return null;
        if(args == null || args.Length == 0) return txt;
        try { return string.Format(txt, args); } catch { return txt; }
    }

    internal static void Write(string section, string txt, ConsoleColor? consoleColor = null, ColorARGB? color = null) {
        LogSink.Msg(Prefix(section, txt));
        MsgCallbackHandler?.Invoke(ConsoleColor.Gray, consoleColor ?? ConsoleColor.Gray, section, txt);
        MsgDrawingCallbackHandler?.Invoke(DefaultMelonColor, color ?? DefaultTextColor, section, txt);
    }

    internal static void Warn(string section, string txt) {
        LogSink.Warning(Prefix(section, txt));
        WarningCallbackHandler?.Invoke(section, txt);
    }

    internal static void Err(string section, string txt) {
        LogSink.Error(Prefix(section, txt));
        ErrorCallbackHandler?.Invoke(section, txt);
    }

    private static string Prefix(string section, string txt) =>
        string.IsNullOrEmpty(section) ? txt ?? string.Empty : $"[{section}] {txt}";

    // The per-melon logger. Every mod gets one named after itself so its output
    // stays attributable in a shared log file.
    public class Instance {
        private readonly string name;

        public Instance(string name) => this.name = name;
        public Instance(string name, ConsoleColor color) : this(name) => Color = ColorARGB.FromConsoleColor(color);
        public Instance(string name, ColorARGB color) : this(name) => Color = color;

        public ColorARGB Color { get; set; } = DefaultMelonColor;

        public void Msg(object obj) => Write(name, obj?.ToString());
        public void Msg(string txt) => Write(name, txt);
        public void Msg(string txt, params object[] args) => Write(name, Format(txt, args));
        public void Msg(ConsoleColor txtColor, object obj) => Write(name, obj?.ToString(), txtColor);
        public void Msg(ConsoleColor txtColor, string txt) => Write(name, txt, txtColor);
        public void Msg(ConsoleColor txtColor, string txt, params object[] args) => Write(name, Format(txt, args), txtColor);
        public void Msg(ColorARGB txtColor, object obj) => Write(name, obj?.ToString(), null, txtColor);
        public void Msg(ColorARGB txtColor, string txt) => Write(name, txt, null, txtColor);
        public void Msg(ColorARGB txtColor, string txt, params object[] args) => Write(name, Format(txt, args), null, txtColor);

        public void MsgPastel(object obj) => Msg(obj);
        public void MsgPastel(string txt) => Msg(txt);
        public void MsgPastel(string txt, params object[] args) => Msg(txt, args);
        public void MsgPastel(ConsoleColor txtColor, object obj) => Msg(txtColor, obj);
        public void MsgPastel(ConsoleColor txtColor, string txt) => Msg(txtColor, txt);
        public void MsgPastel(ConsoleColor txtColor, string txt, params object[] args) => Msg(txtColor, txt, args);
        public void MsgPastel(ColorARGB txtColor, object obj) => Msg(txtColor, obj);
        public void MsgPastel(ColorARGB txtColor, string txt) => Msg(txtColor, txt);
        public void MsgPastel(ColorARGB txtColor, string txt, params object[] args) => Msg(txtColor, txt, args);

        public void Warning(object obj) => Warn(name, obj?.ToString());
        public void Warning(string txt) => Warn(name, txt);
        public void Warning(string txt, params object[] args) => Warn(name, Format(txt, args));

        public void Error(object obj) => Err(name, obj?.ToString());
        public void Error(string txt) => Err(name, txt);
        public void Error(string txt, params object[] args) => Err(name, Format(txt, args));
        public void Error(string txt, Exception ex) => Err(name, txt + "\n" + ex);

        public void WriteSpacer() => Write(name, string.Empty);
        public void WriteLine(int length = 30) => Write(name, new string('-', Math.Max(1, length)));
        public void WriteLine(ColorARGB color, int length = 30) => Write(name, new string('-', Math.Max(1, length)), null, color);
        public void BigError(string txt) => MelonLogger.BigError(name, txt);
    }
}
