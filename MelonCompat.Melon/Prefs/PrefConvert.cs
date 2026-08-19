using System.Collections;
using System.Globalization;
using System.Linq;

namespace MelonLoader.Preferences;

// TOML only carries strings, booleans, integers, floats and flat arrays, but mods
// declare preferences of any type. This is the bridge in both directions, and it
// falls back to the declared default rather than throwing on a malformed file —
// a hand-edited config should cost one setting, not the whole mod.
internal static class PrefConvert {
    public static T To<T>(object raw, T fallback) =>
        To(raw, typeof(T), fallback) is T typed ? typed : fallback;

    public static object To(object raw, Type target, object fallback) {
        if(raw == null) return fallback;
        if(target.IsInstanceOfType(raw)) return raw;

        Type underlying = Nullable.GetUnderlyingType(target) ?? target;

        if(underlying.IsEnum) {
            try {
                return raw is string text ? Enum.Parse(underlying, text, true) : Enum.ToObject(underlying, raw);
            } catch {
                return fallback;
            }
        }

        if(raw is IList list && underlying != typeof(string)) {
            Type element = underlying.IsArray ? underlying.GetElementType()
                : underlying.IsGenericType ? underlying.GetGenericArguments().FirstOrDefault()
                : null;
            if(element == null) return fallback;

            Array items = Array.CreateInstance(element, list.Count);
            for(int i = 0; i < list.Count; i++) items.SetValue(To(list[i], element, Default(element)), i);
            if(underlying.IsArray) return items;
            try {
                return Activator.CreateInstance(underlying, items);
            } catch {
                return fallback;
            }
        }

        try {
            return System.Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
        } catch {
            return fallback;
        }
    }

    // Enums and anything exotic go out as strings so the file stays readable and
    // survives a round trip.
    public static object Box(object source) {
        if(source == null) return null;
        Type type = source.GetType();
        if(type.IsEnum) return source.ToString();
        if(source is string or bool) return source;
        if(type.IsPrimitive || source is decimal) return source;
        if(source is IEnumerable enumerable) return enumerable.Cast<object>().Select(Box).ToList();
        return source.ToString();
    }

    private static object Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
}
