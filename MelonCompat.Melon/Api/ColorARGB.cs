namespace MelonLoader.Logging;

// MelonLoader logs in 32-bit colour; UnityModManager's log is plain text. The
// value is kept faithfully so mods that compare or store colours behave, and the
// log writer renders it as a Unity rich-text hex tag.
public readonly struct ColorARGB : IEquatable<ColorARGB> {
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    private ColorARGB(byte a, byte r, byte g, byte b) {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public static ColorARGB FromArgb(byte r, byte g, byte b) => new(255, r, g, b);
    public static ColorARGB FromArgb(byte a, byte r, byte g, byte b) => new(a, r, g, b);
    public static ColorARGB FromArgb(byte a, ColorARGB color) => new(a, color.R, color.G, color.B);

    public static ColorARGB FromArgb(uint argb) =>
        new((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    public uint ToArgb() => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;

    // Unity's rich text only understands RGB(A) hex, which is what the log needs.
    public string ToHex() => $"{R:X2}{G:X2}{B:X2}";

    public static implicit operator ColorARGB(ConsoleColor color) => FromConsoleColor(color);

    public static ColorARGB FromConsoleColor(ConsoleColor color) => color switch {
        ConsoleColor.Black => Black,
        ConsoleColor.DarkBlue => FromArgb(0, 0, 139),
        ConsoleColor.DarkGreen => DarkGreen,
        ConsoleColor.DarkCyan => DarkCyan,
        ConsoleColor.DarkRed => DarkRed,
        ConsoleColor.DarkMagenta => DarkMagenta,
        ConsoleColor.DarkYellow => FromArgb(128, 128, 0),
        ConsoleColor.Gray => Gray,
        ConsoleColor.DarkGray => DarkGray,
        ConsoleColor.Blue => Blue,
        ConsoleColor.Green => Green,
        ConsoleColor.Cyan => Cyan,
        ConsoleColor.Red => Red,
        ConsoleColor.Magenta => Magenta,
        ConsoleColor.Yellow => Yellow,
        _ => White,
    };

    public bool Equals(ColorARGB other) => ToArgb() == other.ToArgb();
    public override bool Equals(object obj) => obj is ColorARGB other && Equals(other);
    public override int GetHashCode() => (int)ToArgb();
    public override string ToString() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    public static bool operator ==(ColorARGB left, ColorARGB right) => left.Equals(right);
    public static bool operator !=(ColorARGB left, ColorARGB right) => !left.Equals(right);

    public static ColorARGB Transparent => FromArgb(0x00FFFFFFu);
    public static ColorARGB AliceBlue => FromArgb(0xFFF0F8FFu);
    public static ColorARGB AntiqueWhite => FromArgb(0xFFFAEBD7u);
    public static ColorARGB Aqua => FromArgb(0xFF00FFFFu);
    public static ColorARGB Aquamarine => FromArgb(0xFF7FFFD4u);
    public static ColorARGB Azure => FromArgb(0xFFF0FFFFu);
    public static ColorARGB Beige => FromArgb(0xFFF5F5DCu);
    public static ColorARGB Bisque => FromArgb(0xFFFFE4C4u);
    public static ColorARGB Black => FromArgb(0xFF000000u);
    public static ColorARGB BlanchedAlmond => FromArgb(0xFFFFEBCDu);
    public static ColorARGB Blue => FromArgb(0xFF0000FFu);
    public static ColorARGB BlueViolet => FromArgb(0xFF8A2BE2u);
    public static ColorARGB Brown => FromArgb(0xFFA52A2Au);
    public static ColorARGB BurlyWood => FromArgb(0xFFDEB887u);
    public static ColorARGB CadetBlue => FromArgb(0xFF5F9EA0u);
    public static ColorARGB Chartreuse => FromArgb(0xFF7FFF00u);
    public static ColorARGB Chocolate => FromArgb(0xFFD2691Eu);
    public static ColorARGB Coral => FromArgb(0xFFFF7F50u);
    public static ColorARGB CornflowerBlue => FromArgb(0xFF6495EDu);
    public static ColorARGB Cornsilk => FromArgb(0xFFFFF8DCu);
    public static ColorARGB Crimson => FromArgb(0xFFDC143Cu);
    public static ColorARGB Cyan => FromArgb(0xFF00FFFFu);
    public static ColorARGB DarkBlue => FromArgb(0xFF00008Bu);
    public static ColorARGB DarkCyan => FromArgb(0xFF008B8Bu);
    public static ColorARGB DarkGoldenrod => FromArgb(0xFFB8860Bu);
    public static ColorARGB DarkGray => FromArgb(0xFFA9A9A9u);
    public static ColorARGB DarkGreen => FromArgb(0xFF006400u);
    public static ColorARGB DarkKhaki => FromArgb(0xFFBDB76Bu);
    public static ColorARGB DarkMagenta => FromArgb(0xFF8B008Bu);
    public static ColorARGB DarkOliveGreen => FromArgb(0xFF556B2Fu);
    public static ColorARGB DarkOrange => FromArgb(0xFFFF8C00u);
    public static ColorARGB DarkOrchid => FromArgb(0xFF9932CCu);
    public static ColorARGB DarkRed => FromArgb(0xFF8B0000u);
    public static ColorARGB DarkSalmon => FromArgb(0xFFE9967Au);
    public static ColorARGB DarkSeaGreen => FromArgb(0xFF8FBC8Fu);
    public static ColorARGB DarkSlateBlue => FromArgb(0xFF483D8Bu);
    public static ColorARGB DarkSlateGray => FromArgb(0xFF2F4F4Fu);
    public static ColorARGB DarkTurquoise => FromArgb(0xFF00CED1u);
    public static ColorARGB DarkViolet => FromArgb(0xFF9400D3u);
    public static ColorARGB DeepPink => FromArgb(0xFFFF1493u);
    public static ColorARGB DeepSkyBlue => FromArgb(0xFF00BFFFu);
    public static ColorARGB DimGray => FromArgb(0xFF696969u);
    public static ColorARGB DodgerBlue => FromArgb(0xFF1E90FFu);
    public static ColorARGB Firebrick => FromArgb(0xFFB22222u);
    public static ColorARGB FloralWhite => FromArgb(0xFFFFFAF0u);
    public static ColorARGB ForestGreen => FromArgb(0xFF228B22u);
    public static ColorARGB Fuchsia => FromArgb(0xFFFF00FFu);
    public static ColorARGB Gainsboro => FromArgb(0xFFDCDCDCu);
    public static ColorARGB GhostWhite => FromArgb(0xFFF8F8FFu);
    public static ColorARGB Gold => FromArgb(0xFFFFD700u);
    public static ColorARGB Goldenrod => FromArgb(0xFFDAA520u);
    public static ColorARGB Gray => FromArgb(0xFF808080u);
    public static ColorARGB Green => FromArgb(0xFF008000u);
    public static ColorARGB GreenYellow => FromArgb(0xFFADFF2Fu);
    public static ColorARGB Honeydew => FromArgb(0xFFF0FFF0u);
    public static ColorARGB HotPink => FromArgb(0xFFFF69B4u);
    public static ColorARGB IndianRed => FromArgb(0xFFCD5C5Cu);
    public static ColorARGB Indigo => FromArgb(0xFF4B0082u);
    public static ColorARGB Ivory => FromArgb(0xFFFFFFF0u);
    public static ColorARGB Khaki => FromArgb(0xFFF0E68Cu);
    public static ColorARGB Lavender => FromArgb(0xFFE6E6FAu);
    public static ColorARGB LavenderBlush => FromArgb(0xFFFFF0F5u);
    public static ColorARGB LawnGreen => FromArgb(0xFF7CFC00u);
    public static ColorARGB LemonChiffon => FromArgb(0xFFFFFACDu);
    public static ColorARGB LightBlue => FromArgb(0xFFADD8E6u);
    public static ColorARGB LightCoral => FromArgb(0xFFF08080u);
    public static ColorARGB LightCyan => FromArgb(0xFFE0FFFFu);
    public static ColorARGB LightGoldenrodYellow => FromArgb(0xFFFAFAD2u);
    public static ColorARGB LightGreen => FromArgb(0xFF90EE90u);
    public static ColorARGB LightGray => FromArgb(0xFFD3D3D3u);
    public static ColorARGB LightPink => FromArgb(0xFFFFB6C1u);
    public static ColorARGB LightSalmon => FromArgb(0xFFFFA07Au);
    public static ColorARGB LightSeaGreen => FromArgb(0xFF20B2AAu);
    public static ColorARGB LightSkyBlue => FromArgb(0xFF87CEFAu);
    public static ColorARGB LightSlateGray => FromArgb(0xFF778899u);
    public static ColorARGB LightSteelBlue => FromArgb(0xFFB0C4DEu);
    public static ColorARGB LightYellow => FromArgb(0xFFFFFFE0u);
    public static ColorARGB Lime => FromArgb(0xFF00FF00u);
    public static ColorARGB LimeGreen => FromArgb(0xFF32CD32u);
    public static ColorARGB Linen => FromArgb(0xFFFAF0E6u);
    public static ColorARGB Magenta => FromArgb(0xFFFF00FFu);
    public static ColorARGB Maroon => FromArgb(0xFF800000u);
    public static ColorARGB MediumAquamarine => FromArgb(0xFF66CDAAu);
    public static ColorARGB MediumBlue => FromArgb(0xFF0000CDu);
    public static ColorARGB MediumOrchid => FromArgb(0xFFBA55D3u);
    public static ColorARGB MediumPurple => FromArgb(0xFF9370DBu);
    public static ColorARGB MediumSeaGreen => FromArgb(0xFF3CB371u);
    public static ColorARGB MediumSlateBlue => FromArgb(0xFF7B68EEu);
    public static ColorARGB MediumSpringGreen => FromArgb(0xFF00FA9Au);
    public static ColorARGB MediumTurquoise => FromArgb(0xFF48D1CCu);
    public static ColorARGB MediumVioletRed => FromArgb(0xFFC71585u);
    public static ColorARGB MidnightBlue => FromArgb(0xFF191970u);
    public static ColorARGB MintCream => FromArgb(0xFFF5FFFAu);
    public static ColorARGB MistyRose => FromArgb(0xFFFFE4E1u);
    public static ColorARGB Moccasin => FromArgb(0xFFFFE4B5u);
    public static ColorARGB NavajoWhite => FromArgb(0xFFFFDEADu);
    public static ColorARGB Navy => FromArgb(0xFF000080u);
    public static ColorARGB OldLace => FromArgb(0xFFFDF5E6u);
    public static ColorARGB Olive => FromArgb(0xFF808000u);
    public static ColorARGB OliveDrab => FromArgb(0xFF6B8E23u);
    public static ColorARGB Orange => FromArgb(0xFFFFA500u);
    public static ColorARGB OrangeRed => FromArgb(0xFFFF4500u);
    public static ColorARGB Orchid => FromArgb(0xFFDA70D6u);
    public static ColorARGB PaleGoldenrod => FromArgb(0xFFEEE8AAu);
    public static ColorARGB PaleGreen => FromArgb(0xFF98FB98u);
    public static ColorARGB PaleTurquoise => FromArgb(0xFFAFEEEEu);
    public static ColorARGB PaleVioletRed => FromArgb(0xFFDB7093u);
    public static ColorARGB PapayaWhip => FromArgb(0xFFFFEFD5u);
    public static ColorARGB PeachPuff => FromArgb(0xFFFFDAB9u);
    public static ColorARGB Peru => FromArgb(0xFFCD853Fu);
    public static ColorARGB Pink => FromArgb(0xFFFFC0CBu);
    public static ColorARGB Plum => FromArgb(0xFFDDA0DDu);
    public static ColorARGB PowderBlue => FromArgb(0xFFB0E0E6u);
    public static ColorARGB Purple => FromArgb(0xFF800080u);
    public static ColorARGB RebeccaPurple => FromArgb(0xFF663399u);
    public static ColorARGB Red => FromArgb(0xFFFF0000u);
    public static ColorARGB RosyBrown => FromArgb(0xFFBC8F8Fu);
    public static ColorARGB RoyalBlue => FromArgb(0xFF4169E1u);
    public static ColorARGB SaddleBrown => FromArgb(0xFF8B4513u);
    public static ColorARGB Salmon => FromArgb(0xFFFA8072u);
    public static ColorARGB SandyBrown => FromArgb(0xFFF4A460u);
    public static ColorARGB SeaGreen => FromArgb(0xFF2E8B57u);
    public static ColorARGB SeaShell => FromArgb(0xFFFFF5EEu);
    public static ColorARGB Sienna => FromArgb(0xFFA0522Du);
    public static ColorARGB Silver => FromArgb(0xFFC0C0C0u);
    public static ColorARGB SkyBlue => FromArgb(0xFF87CEEBu);
    public static ColorARGB SlateBlue => FromArgb(0xFF6A5ACDu);
    public static ColorARGB SlateGray => FromArgb(0xFF708090u);
    public static ColorARGB Snow => FromArgb(0xFFFFFAFAu);
    public static ColorARGB SpringGreen => FromArgb(0xFF00FF7Fu);
    public static ColorARGB SteelBlue => FromArgb(0xFF4682B4u);
    public static ColorARGB Tan => FromArgb(0xFFD2B48Cu);
    public static ColorARGB Teal => FromArgb(0xFF008080u);
    public static ColorARGB Thistle => FromArgb(0xFFD8BFD8u);
    public static ColorARGB Tomato => FromArgb(0xFFFF6347u);
    public static ColorARGB Turquoise => FromArgb(0xFF40E0D0u);
    public static ColorARGB Violet => FromArgb(0xFFEE82EEu);
    public static ColorARGB Wheat => FromArgb(0xFFF5DEB3u);
    public static ColorARGB White => FromArgb(0xFFFFFFFFu);
    public static ColorARGB WhiteSmoke => FromArgb(0xFFF5F5F5u);
    public static ColorARGB Yellow => FromArgb(0xFFFFFF00u);
    public static ColorARGB YellowGreen => FromArgb(0xFF9ACD32u);
}
