using System.Linq;

namespace Semver;

// MelonLoader vendors Semver inside MelonLoader.dll, so mods reference
// Semver.SemVersion without a second assembly. Only what the loader surface
// actually exposes is implemented: parse, compare, print.
public sealed class SemVersion : IComparable<SemVersion>, IEquatable<SemVersion> {
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string Prerelease { get; }
    public string Build { get; }

    public SemVersion(int major, int minor = 0, int patch = 0, string prerelease = "", string build = "") {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease ?? "";
        Build = build ?? "";
    }

    public static SemVersion Parse(string version, bool strict = false) =>
        TryParse(version, out SemVersion parsed)
            ? parsed
            : strict ? throw new ArgumentException($"'{version}' is not a semantic version", nameof(version)) : new SemVersion(0);

    public static bool TryParse(string version, out SemVersion result) {
        result = null;
        if(string.IsNullOrEmpty(version)) return false;

        string core = version.Trim().TrimStart('v', 'V');
        string build = "";
        int plus = core.IndexOf('+');
        if(plus >= 0) {
            build = core[(plus + 1)..];
            core = core[..plus];
        }
        string prerelease = "";
        int dash = core.IndexOf('-');
        if(dash >= 0) {
            prerelease = core[(dash + 1)..];
            core = core[..dash];
        }

        string[] parts = core.Split('.');
        if(parts.Length == 0 || !int.TryParse(parts[0], out int major)) return false;
        int minor = parts.Length > 1 && int.TryParse(parts[1], out int m) ? m : 0;
        int patch = parts.Length > 2 && int.TryParse(parts[2], out int p) ? p : 0;
        result = new SemVersion(major, minor, patch, prerelease, build);
        return true;
    }

    public int CompareTo(SemVersion other) {
        if(other == null) return 1;
        int result = Major.CompareTo(other.Major);
        if(result != 0) return result;
        result = Minor.CompareTo(other.Minor);
        if(result != 0) return result;
        result = Patch.CompareTo(other.Patch);
        if(result != 0) return result;
        return ComparePrerelease(Prerelease, other.Prerelease);
    }

    // A release outranks any prerelease of the same core version; beyond that the
    // dot-separated identifiers compare numerically when both are numeric.
    private static int ComparePrerelease(string left, string right) {
        bool leftEmpty = string.IsNullOrEmpty(left);
        bool rightEmpty = string.IsNullOrEmpty(right);
        if(leftEmpty && rightEmpty) return 0;
        if(leftEmpty) return 1;
        if(rightEmpty) return -1;

        string[] leftParts = left.Split('.');
        string[] rightParts = right.Split('.');
        for(int i = 0; i < Math.Max(leftParts.Length, rightParts.Length); i++) {
            if(i >= leftParts.Length) return -1;
            if(i >= rightParts.Length) return 1;
            bool leftNumeric = int.TryParse(leftParts[i], out int leftValue);
            bool rightNumeric = int.TryParse(rightParts[i], out int rightValue);
            int result = leftNumeric && rightNumeric
                ? leftValue.CompareTo(rightValue)
                : leftNumeric ? -1
                : rightNumeric ? 1
                : string.CompareOrdinal(leftParts[i], rightParts[i]);
            if(result != 0) return result;
        }
        return 0;
    }

    public bool Equals(SemVersion other) => CompareTo(other) == 0;
    public override bool Equals(object obj) => obj is SemVersion other && Equals(other);
    public override int GetHashCode() => (Major, Minor, Patch, Prerelease).GetHashCode();

    public override string ToString() {
        string text = $"{Major}.{Minor}.{Patch}";
        if(!string.IsNullOrEmpty(Prerelease)) text += "-" + Prerelease;
        if(!string.IsNullOrEmpty(Build)) text += "+" + Build;
        return text;
    }

    public static bool operator ==(SemVersion left, SemVersion right) => Equals(left, right) || (left?.Equals(right) ?? false);
    public static bool operator !=(SemVersion left, SemVersion right) => !(left == right);
    public static bool operator >(SemVersion left, SemVersion right) => (left?.CompareTo(right) ?? -1) > 0;
    public static bool operator <(SemVersion left, SemVersion right) => (left?.CompareTo(right) ?? -1) < 0;
    public static bool operator >=(SemVersion left, SemVersion right) => (left?.CompareTo(right) ?? -1) >= 0;
    public static bool operator <=(SemVersion left, SemVersion right) => (left?.CompareTo(right) ?? -1) <= 0;
}
