using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MelonLoader;

// MelonLoader's own tuple family. Each arity extends the one below it, so Item1
// lives on LemonTuple<T1> and every larger tuple inherits it — a mod that stores a
// LemonTuple<A,B> in a LemonTuple<A> depends on exactly that shape.
public class LemonTuple<T1> {
    public T1 Item1;
    public LemonTuple() { }
    public LemonTuple(T1 item1) => Item1 = item1;
}

public class LemonTuple<T1, T2> : LemonTuple<T1> {
    public T2 Item2;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2) : base(item1) => Item2 = item2;
}

public class LemonTuple<T1, T2, T3> : LemonTuple<T1, T2> {
    public T3 Item3;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3) : base(item1, item2) => Item3 = item3;
}

public class LemonTuple<T1, T2, T3, T4> : LemonTuple<T1, T2, T3> {
    public T4 Item4;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4) : base(item1, item2, item3) => Item4 = item4;
}

public class LemonTuple<T1, T2, T3, T4, T5> : LemonTuple<T1, T2, T3, T4> {
    public T5 Item5;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) : base(item1, item2, item3, item4) => Item5 = item5;
}

public class LemonTuple<T1, T2, T3, T4, T5, T6> : LemonTuple<T1, T2, T3, T4, T5> {
    public T6 Item6;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) : base(item1, item2, item3, item4, item5) => Item6 = item6;
}

public class LemonTuple<T1, T2, T3, T4, T5, T6, T7> : LemonTuple<T1, T2, T3, T4, T5, T6> {
    public T7 Item7;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) : base(item1, item2, item3, item4, item5, item6) => Item7 = item7;
}

public class LemonTuple<T1, T2, T3, T4, T5, T6, T7, T8> : LemonTuple<T1, T2, T3, T4, T5, T6, T7> {
    public T8 Item8;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8) : base(item1, item2, item3, item4, item5, item6, item7) => Item8 = item8;
}

public class LemonTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> : LemonTuple<T1, T2, T3, T4, T5, T6, T7, T8> {
    public T9 Item9;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9) : base(item1, item2, item3, item4, item5, item6, item7, item8) => Item9 = item9;
}

public class LemonTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : LemonTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> {
    public T10 Item10;
    public LemonTuple() { }
    public LemonTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9, T10 item10) : base(item1, item2, item3, item4, item5, item6, item7, item8, item9) => Item10 = item10;
}

// An enumerator that can look at the next element without consuming it.
public class LemonEnumerator<T> : IEnumerator<T>, IEnumerable<T> {
    private readonly IList<T> source;
    private int index = -1;

    public LemonEnumerator(T[] source) => this.source = source ?? [];
    public LemonEnumerator(IList<T> source) => this.source = source ?? [];

    public T Current => index >= 0 && index < source.Count ? source[index] : default;
    object IEnumerator.Current => Current;

    public bool MoveNext() => ++index < source.Count;

    public bool Peek(out T value) {
        if(index + 1 < source.Count) {
            value = source[index + 1];
            return true;
        }
        value = default;
        return false;
    }

    public void Reset() => index = -1;
    public void Dispose() { }
    public IEnumerator<T> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;
}

// A window onto part of an array, without copying it.
public readonly struct LemonArraySegment<T> : IEquatable<LemonArraySegment<T>> {
    public T[] Array { get; }
    public int Offset { get; }
    public int Count { get; }

    public LemonArraySegment(T[] array) : this(array, 0, array?.Length ?? 0) { }

    public LemonArraySegment(T[] array, int offset, int count) {
        Array = array;
        Offset = offset;
        Count = count;
    }

    public T this[int index] => Array[Offset + index];

    public bool Equals(LemonArraySegment<T> other) =>
        ReferenceEquals(Array, other.Array) && Offset == other.Offset && Count == other.Count;

    public override bool Equals(object obj) => obj is LemonArraySegment<T> other && Equals(other);
    public override int GetHashCode() => (Array?.GetHashCode() ?? 0) ^ Offset ^ (Count << 16);

    public static bool operator ==(LemonArraySegment<T> left, LemonArraySegment<T> right) => left.Equals(right);
    public static bool operator !=(LemonArraySegment<T> left, LemonArraySegment<T> right) => !left.Equals(right);
}

public static class EnumExtensions {
    // Predates Enum.HasFlag being usable on the loader's target framework.
    public static bool HasFlag(this Enum value, Enum flag) =>
        (Convert.ToInt64(value) & Convert.ToInt64(flag)) == Convert.ToInt64(flag);
}
