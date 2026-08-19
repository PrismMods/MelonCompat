using System.Globalization;
using System.Linq;
using System.Text;

namespace MelonLoader.Preferences;

// MelonPreferences files are TOML, and mods expect their existing
// UserData/MelonPreferences.cfg to keep working after switching loaders. This is
// the subset that file actually uses: tables, scalars, and flat arrays.
internal static class Toml {
    public static Dictionary<string, Dictionary<string, object>> Parse(string text) {
        Dictionary<string, Dictionary<string, object>> tables = new(StringComparer.Ordinal);
        if(string.IsNullOrEmpty(text)) return tables;

        Dictionary<string, object> current = null;
        foreach(string rawLine in text.Split('\n')) {
            string line = rawLine.Trim().TrimEnd('\r');
            if(line.Length == 0 || line[0] == '#') continue;

            if(line[0] == '[') {
                int end = line.IndexOf(']');
                if(end < 0) continue;
                string name = line.Substring(1, end - 1).Trim().Trim('"');
                if(!tables.TryGetValue(name, out current)) tables[name] = current = new Dictionary<string, object>(StringComparer.Ordinal);
                continue;
            }

            int equals = line.IndexOf('=');
            if(equals < 0 || current == null) continue;
            string key = line[..equals].Trim().Trim('"');
            string value = StripComment(line[(equals + 1)..].Trim());
            if(key.Length == 0) continue;
            current[key] = ParseValue(value);
        }
        return tables;
    }

    // A '#' inside a quoted string is data, not a comment.
    private static string StripComment(string value) {
        bool quoted = false;
        for(int i = 0; i < value.Length; i++) {
            if(value[i] == '"' && (i == 0 || value[i - 1] != '\\')) quoted = !quoted;
            else if(value[i] == '#' && !quoted) return value[..i].TrimEnd();
        }
        return value;
    }

    private static object ParseValue(string value) {
        if(value.Length == 0) return "";
        if(value[0] == '"') return Unescape(value.Trim('"'));
        if(value[0] == '\'') return value.Trim('\'');
        if(value[0] == '[') return ParseArray(value);
        if(value == "true") return true;
        if(value == "false") return false;
        if(long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer)) return integer;
        if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double real)) return real;
        return value;
    }

    private static List<object> ParseArray(string value) {
        List<object> items = [];
        string inner = value.Trim();
        if(inner.StartsWith("[")) inner = inner[1..];
        if(inner.EndsWith("]")) inner = inner[..^1];

        StringBuilder item = new();
        bool quoted = false;
        int depth = 0;
        foreach(char c in inner) {
            if(c == '"') quoted = !quoted;
            if(!quoted) {
                if(c == '[') depth++;
                else if(c == ']') depth--;
                else if(c == ',' && depth == 0) {
                    AddItem(items, item);
                    continue;
                }
            }
            item.Append(c);
        }
        AddItem(items, item);
        return items;
    }

    private static void AddItem(List<object> items, StringBuilder item) {
        string text = item.ToString().Trim();
        item.Clear();
        if(text.Length > 0) items.Add(ParseValue(text));
    }

    public static string Write(IEnumerable<TomlTable> tables) {
        StringBuilder builder = new();
        foreach(TomlTable table in tables) {
            if(!string.IsNullOrEmpty(table.Comment))
                foreach(string line in table.Comment.Split('\n'))
                    builder.Append("# ").AppendLine(line.TrimEnd('\r'));
            builder.Append('[').Append(table.Name).AppendLine("]");
            foreach(TomlEntry entry in table.Entries) {
                if(!string.IsNullOrEmpty(entry.Comment))
                    foreach(string line in entry.Comment.Split('\n'))
                        builder.Append("# ").AppendLine(line.TrimEnd('\r'));
                builder.Append(entry.Key).Append(" = ").AppendLine(Format(entry.Value));
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }

    public static string Format(object value) => value switch {
        null => "\"\"",
        bool boolean => boolean ? "true" : "false",
        string text => "\"" + Escape(text) + "\"",
        float single => single.ToString("R", CultureInfo.InvariantCulture),
        double real => real.ToString("R", CultureInfo.InvariantCulture),
        decimal dec => dec.ToString(CultureInfo.InvariantCulture),
        IFormattable formattable when IsInteger(value) => formattable.ToString(null, CultureInfo.InvariantCulture),
        System.Collections.IEnumerable list => "[" + string.Join(", ", list.Cast<object>().Select(Format).ToArray()) + "]",
        _ => "\"" + Escape(value.ToString()) + "\"",
    };

    private static bool IsInteger(object value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong;

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

    private static string Unescape(string text) =>
        text.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\\\", "\\");
}

internal sealed class TomlTable {
    public string Name;
    public string Comment;
    public List<TomlEntry> Entries = [];
}

internal sealed class TomlEntry {
    public string Key;
    public object Value;
    public string Comment;
}
