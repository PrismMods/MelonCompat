namespace MelonLoader.Assertions;

public class LemonAssertException : Exception {
    public LemonAssertException(string message) : base(message) { }
}

internal enum LemonAssertMapping {
    IsTrue,
    IsFalse,
    IsNull,
    IsNotNull,
    IsEqual,
    IsNotEqual,
}

// A failed assertion logs by default and only throws when the mod asks it to, so
// one bad assumption does not take the game down with it.
public static class LemonAssert {
    public static void IsTrue(bool value) => Check(value, "the value was false", null, false);
    public static void IsTrue(bool value, string message) => Check(value, "the value was false", message, false);
    public static void IsTrue(bool value, string message, bool shouldThrow) => Check(value, "the value was false", message, shouldThrow);

    public static void IsFalse(bool value) => Check(!value, "the value was true", null, false);
    public static void IsFalse(bool value, string message) => Check(!value, "the value was true", message, false);
    public static void IsFalse(bool value, string message, bool shouldThrow) => Check(!value, "the value was true", message, shouldThrow);

    public static void IsNull<T>(T value) => Check(value == null, "the value was not null", null, false);
    public static void IsNull<T>(T value, string message) => Check(value == null, "the value was not null", message, false);
    public static void IsNull<T>(T value, string message, bool shouldThrow) => Check(value == null, "the value was not null", message, shouldThrow);

    public static void IsNotNull<T>(T value) => Check(value != null, "the value was null", null, false);
    public static void IsNotNull<T>(T value, string message) => Check(value != null, "the value was null", message, false);
    public static void IsNotNull<T>(T value, string message, bool shouldThrow) => Check(value != null, "the value was null", message, shouldThrow);

    public static void IsEqual<T>(T left, T right) => Check(Equals(left, right), $"{left} != {right}", null, false);
    public static void IsEqual<T>(T left, T right, string message) => Check(Equals(left, right), $"{left} != {right}", message, false);
    public static void IsEqual<T>(T left, T right, string message, bool shouldThrow) => Check(Equals(left, right), $"{left} != {right}", message, shouldThrow);

    public static void IsNotEqual<T>(T left, T right) => Check(!Equals(left, right), $"{left} == {right}", null, false);
    public static void IsNotEqual<T>(T left, T right, string message) => Check(!Equals(left, right), $"{left} == {right}", message, false);
    public static void IsNotEqual<T>(T left, T right, string message, bool shouldThrow) => Check(!Equals(left, right), $"{left} == {right}", message, shouldThrow);

    private static void Check(bool passed, string reason, string message, bool shouldThrow) {
        if(passed) return;
        string text = string.IsNullOrEmpty(message) ? $"assertion failed: {reason}" : $"assertion failed: {message} ({reason})";
        if(shouldThrow) throw new LemonAssertException(text);
        MelonLogger.Error(text);
    }
}
