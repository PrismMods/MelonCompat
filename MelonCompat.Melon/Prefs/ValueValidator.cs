namespace MelonLoader.Preferences;

// A preference can refuse a value. Entries run every incoming value through the
// validator, so a config file edited by hand cannot push a mod out of range.
public abstract class ValueValidator {
    public abstract bool IsValid(object value);
    public abstract object EnsureValid(object value);
}

public interface IValueRange {
    object MinValue { get; }
    object MaxValue { get; }
}

public class ValueRange<T> : ValueValidator, IValueRange where T : IComparable<T> {
    public T Min { get; }
    public T Max { get; }

    public object MinValue => Min;
    public object MaxValue => Max;

    public ValueRange(T min, T max) {
        if(min.CompareTo(max) > 0) throw new ArgumentException("the minimum of a ValueRange cannot exceed its maximum");
        Min = min;
        Max = max;
    }

    public override bool IsValid(object value) =>
        value is T typed && typed.CompareTo(Min) >= 0 && typed.CompareTo(Max) <= 0;

    public override object EnsureValid(object value) {
        if(value is not T typed) return Min;
        return typed.CompareTo(Min) < 0 ? Min : typed.CompareTo(Max) > 0 ? Max : typed;
    }
}
