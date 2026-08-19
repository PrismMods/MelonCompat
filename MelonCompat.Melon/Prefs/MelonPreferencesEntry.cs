using System.Collections;
using System.Globalization;
using System.Linq;
using MelonLoader.Preferences;

namespace MelonLoader;

// The untyped half of a preference, so categories can hold a mixed list.
public abstract class MelonPreferences_Entry {
    public readonly MelonEvent<object, object> OnEntryValueChangedUntyped = new();

    public string Identifier { get; internal set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Comment { get; set; }
    public bool IsHidden { get; set; }
    public bool DontSaveDefault { get; set; }
    public MelonPreferences_Category Category { get; internal set; }
    public ValueValidator Validator { get; internal set; }

    public abstract object BoxedValue { get; set; }
    public abstract object BoxedEditedValue { get; set; }
    public abstract Type GetReflectedType();
    public abstract void ResetToDefault();
    public abstract string GetValueAsString();
    public abstract string GetEditedValueAsString();
    public abstract string GetDefaultValueAsString();

    internal abstract void LoadRaw(object raw);
    internal abstract object SaveRaw();
    internal abstract bool IsDefault { get; }

    public string GetExceptionMessage(string submsg) =>
        $"preference '{Identifier}' in category '{Category?.Identifier}' {submsg}";

    protected void FireUntypedValueChanged(object old, object neew) => OnEntryValueChangedUntyped.Invoke(old, neew);

    public event Action OnValueChangedUntyped {
        add => OnEntryValueChangedUntyped.Subscribe((_, _) => value());
        remove { }
    }
}

// A typed preference. EditedValue is the staging slot a settings UI writes into;
// Value is what the mod reads. MelonLoader keeps them separate so a UI can offer
// apply and cancel, and mods rely on that split.
public class MelonPreferences_Entry<T> : MelonPreferences_Entry {
    public readonly MelonEvent<T, T> OnEntryValueChanged = new();

    private T value;
    private T editedValue;

    public T DefaultValue { get; set; }

    public T Value {
        get => value;
        set {
            T old = this.value;
            this.value = Coerce(value);
            editedValue = this.value;
            if(Equals(old, this.value)) return;
            OnEntryValueChanged.Invoke(old, this.value);
            FireUntypedValueChanged(old, this.value);
        }
    }

    public T EditedValue {
        get => editedValue;
        set => editedValue = Coerce(value);
    }

    public override object BoxedValue {
        get => Value;
        set => Value = PrefConvert.To(value, DefaultValue);
    }

    public override object BoxedEditedValue {
        get => EditedValue;
        set => EditedValue = PrefConvert.To(value, DefaultValue);
    }

    internal MelonPreferences_Entry(string identifier, T defaultValue) {
        Identifier = identifier;
        DisplayName = identifier;
        DefaultValue = defaultValue;
        value = defaultValue;
        editedValue = defaultValue;
    }

    public override Type GetReflectedType() => typeof(T);
    public override void ResetToDefault() => Value = DefaultValue;
    public override string GetValueAsString() => Preferences.Toml.Format(Value);
    public override string GetEditedValueAsString() => Preferences.Toml.Format(EditedValue);
    public override string GetDefaultValueAsString() => Preferences.Toml.Format(DefaultValue);

    public void Save() => Category?.SaveToFile(false);

    public event Action<T, T> OnValueChanged {
        add => OnEntryValueChanged.Subscribe(new LemonAction<T, T>(value));
        remove { }
    }

    internal override bool IsDefault => Equals(value, DefaultValue);
    internal override object SaveRaw() => PrefConvert.Box(Value);
    internal override void LoadRaw(object raw) => Value = PrefConvert.To(raw, DefaultValue);

    private T Coerce(T candidate) =>
        Validator == null ? candidate : PrefConvert.To(Validator.EnsureValid(candidate), DefaultValue);
}
