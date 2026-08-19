using System.Linq;
using System.Reflection;

namespace MelonLoader.Preferences;

// A category backed by a plain object: every public field or property on it
// becomes a preference. Mods use this when they want a settings struct instead of
// a pile of entry handles.
public class MelonPreferences_ReflectiveCategory {
    private readonly Type type;
    private object instance;

    public string Identifier { get; }
    public string DisplayName { get; set; }
    public string FilePath { get; private set; }

    internal string ResolvedFilePath => FilePath ?? MelonPreferences.DefaultFilePath;

    private MelonPreferences_ReflectiveCategory(Type type, string identifier, string displayName) {
        this.type = type;
        Identifier = identifier;
        DisplayName = displayName ?? identifier;
        instance = Activator.CreateInstance(type);
    }

    internal static MelonPreferences_ReflectiveCategory Create<T>(string identifier, string displayName) where T : new() =>
        new(typeof(T), identifier, displayName);

    public T GetValue<T>() => instance is T typed ? typed : default;

    public void SetFilePath(string filepath, bool autoload = true, bool printmsg = false) {
        FilePath = filepath;
        if(autoload) LoadFromFile(printmsg);
    }

    public void ResetFilePath() {
        FilePath = null;
        LoadFromFile(false);
    }

    public void SaveToFile(bool printmsg = true) => MelonPreferences.SaveFile(ResolvedFilePath, printmsg);
    public void LoadFromFile(bool printmsg = true) => MelonPreferences.LoadFile(ResolvedFilePath, printmsg);
    public void DestroyFileWatcher() { }

    internal void Load(Dictionary<string, Dictionary<string, object>> tables) {
        if(!tables.TryGetValue(Identifier, out Dictionary<string, object> table)) return;
        instance ??= Activator.CreateInstance(type);
        foreach(MemberInfo member in Members()) {
            if(!table.TryGetValue(member.Name, out object raw)) continue;
            try {
                SetValue(member, PrefConvert.To(raw, MemberType(member), GetValue(member)));
            } catch(Exception e) {
                MelonLogger.Warning($"preference '{member.Name}' in category '{Identifier}' could not be read: {e.Message}");
            }
        }
    }

    private static Type MemberType(MemberInfo member) => member switch {
        FieldInfo field => field.FieldType,
        PropertyInfo property => property.PropertyType,
        _ => typeof(object),
    };

    internal void Store(Dictionary<string, Dictionary<string, object>> tables) {
        if(!tables.TryGetValue(Identifier, out Dictionary<string, object> table))
            tables[Identifier] = table = [];
        foreach(MemberInfo member in Members()) table[member.Name] = PrefConvert.Box(GetValue(member));
    }

    private IEnumerable<MemberInfo> Members() =>
        type.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite));

    private object GetValue(MemberInfo member) => member switch {
        FieldInfo field => field.GetValue(instance),
        PropertyInfo property => property.GetValue(instance),
        _ => null,
    };

    private void SetValue(MemberInfo member, object value) {
        switch(member) {
            case FieldInfo field:
                field.SetValue(instance, value);
                break;
            case PropertyInfo property:
                property.SetValue(instance, value);
                break;
        }
    }
}
