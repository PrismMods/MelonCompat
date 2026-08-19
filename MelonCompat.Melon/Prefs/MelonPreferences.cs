using System.IO;
using System.Linq;
using MelonLoader.Preferences;
using MelonLoader.Utils;

namespace MelonLoader;

// The preference store. Values read off disk are kept as raw TOML objects even
// when no entry claims them yet, so a mod that registers its preferences late
// still finds the user's saved value — and so an unloaded mod's settings are not
// wiped out the next time the file is written.
public static class MelonPreferences {
    public static readonly List<MelonPreferences_Category> Categories = [];
    public static readonly List<MelonPreferences_ReflectiveCategory> ReflectiveCategories = [];

    public static readonly MelonEvent<string> OnPreferencesLoaded = new();
    public static readonly MelonEvent<string> OnPreferencesSaved = new();

    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, object>>> stored =
        new(StringComparer.OrdinalIgnoreCase);

    public static string DefaultFilePath => Path.Combine(MelonEnvironment.UserDataDirectory, "MelonPreferences.cfg");

    public static MelonPreferences_Category CreateCategory(string identifier) => CreateCategory(identifier, null);

    public static MelonPreferences_Category CreateCategory(string identifier, string display_name) =>
        CreateCategory(identifier, display_name, false, true);

    public static MelonPreferences_Category CreateCategory(string identifier, string display_name, bool is_hidden, bool should_save) {
        if(string.IsNullOrEmpty(identifier)) throw new ArgumentException("a preference category needs an identifier", nameof(identifier));
        MelonPreferences_Category existing = GetCategory(identifier);
        if(existing != null) return existing;

        MelonPreferences_Category category = new(identifier, display_name, is_hidden, should_save);
        Categories.Add(category);
        return category;
    }

    public static MelonPreferences_ReflectiveCategory CreateCategory<T>(string identifier, string display_name = null) where T : new() {
        MelonPreferences_ReflectiveCategory category = MelonPreferences_ReflectiveCategory.Create<T>(identifier, display_name);
        ReflectiveCategories.Add(category);
        return category;
    }

    public static MelonPreferences_Entry<T> CreateEntry<T>(string category_identifier, string entry_identifier, T default_value, string display_name = null, string description = null, bool is_hidden = false, bool dont_save_default = false, ValueValidator validator = null) =>
        CreateCategory(category_identifier).CreateEntry(entry_identifier, default_value, display_name, description, is_hidden, dont_save_default, validator);

    public static MelonPreferences_Entry<T> CreateEntry<T>(string category_identifier, string entry_identifier, T default_value, string display_name, bool is_hidden) =>
        CreateCategory(category_identifier).CreateEntry(entry_identifier, default_value, display_name, is_hidden);

    public static MelonPreferences_Category GetCategory(string identifier) =>
        Categories.FirstOrDefault(c => c.Identifier == identifier);

    public static MelonPreferences_Entry GetEntry(string category_identifier, string entry_identifier) =>
        GetCategory(category_identifier)?.GetEntry(entry_identifier);

    public static MelonPreferences_Entry<T> GetEntry<T>(string category_identifier, string entry_identifier) =>
        GetCategory(category_identifier)?.GetEntry<T>(entry_identifier);

    public static bool HasEntry(string category_identifier, string entry_identifier) =>
        GetEntry(category_identifier, entry_identifier) != null;

    public static T GetEntryValue<T>(string category_identifier, string entry_identifier) {
        MelonPreferences_Entry<T> entry = GetEntry<T>(category_identifier, entry_identifier);
        return entry == null ? default : entry.Value;
    }

    public static void SetEntryValue<T>(string category_identifier, string entry_identifier, T value) {
        MelonPreferences_Entry<T> entry = GetEntry<T>(category_identifier, entry_identifier)
            ?? throw new KeyNotFoundException($"there is no preference '{entry_identifier}' in category '{category_identifier}'");
        entry.Value = value;
    }

    public static void SaveCategory<T>(string identifier, bool printmsg = true) => GetCategory(identifier)?.SaveToFile(printmsg);

    public static void Save() {
        foreach(string path in AllPaths()) SaveFile(path, false);
    }

    public static void Load() {
        foreach(string path in AllPaths()) LoadFile(path, false);
    }

    private static string[] AllPaths() =>
        Categories.Select(c => c.ResolvedFilePath)
            .Concat(ReflectiveCategories.Select(c => c.ResolvedFilePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    internal static void LoadFile(string path, bool printmsg) {
        Dictionary<string, Dictionary<string, object>> tables;
        try {
            tables = File.Exists(path) ? Toml.Parse(File.ReadAllText(path)) : [];
        } catch(Exception e) {
            MelonLogger.Error($"could not read {path}: {e.Message}");
            return;
        }

        stored[path] = tables;

        foreach(MelonPreferences_Category category in Categories.Where(c => PathEquals(c.ResolvedFilePath, path)))
            foreach(MelonPreferences_Entry entry in category.Entries)
                ApplyStoredValue(category, entry);

        foreach(MelonPreferences_ReflectiveCategory category in ReflectiveCategories.Where(c => PathEquals(c.ResolvedFilePath, path)))
            category.Load(tables);

        if(printmsg) MelonLogger.Msg($"loaded preferences from {path}");
        OnPreferencesLoaded.Invoke(path);
        MelonBase.ExecuteAll(m => {
            m.OnPreferencesLoaded();
            m.OnPreferencesLoaded(path);
        });
    }

    internal static void SaveFile(string path, bool printmsg) {
        if(!stored.TryGetValue(path, out Dictionary<string, Dictionary<string, object>> tables))
            stored[path] = tables = [];

        foreach(MelonPreferences_Category category in Categories.Where(c => c.ShouldSave && PathEquals(c.ResolvedFilePath, path))) {
            if(!tables.TryGetValue(category.Identifier, out Dictionary<string, object> table))
                tables[category.Identifier] = table = [];
            foreach(MelonPreferences_Entry entry in category.Entries) {
                if(entry.DontSaveDefault && entry.IsDefault) {
                    table.Remove(entry.Identifier);
                    continue;
                }
                table[entry.Identifier] = entry.SaveRaw();
            }
        }

        foreach(MelonPreferences_ReflectiveCategory category in ReflectiveCategories.Where(c => PathEquals(c.ResolvedFilePath, path)))
            category.Store(tables);

        try {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Toml.Write(BuildTables(path, tables)));
        } catch(Exception e) {
            MelonLogger.Error($"could not write {path}: {e.Message}");
            return;
        }

        if(printmsg) MelonLogger.Msg($"saved preferences to {path}");
        OnPreferencesSaved.Invoke(path);
        MelonBase.ExecuteAll(m => {
            m.OnPreferencesSaved();
            m.OnPreferencesSaved(path);
        });
    }

    private static IEnumerable<TomlTable> BuildTables(string path, Dictionary<string, Dictionary<string, object>> tables) {
        foreach(KeyValuePair<string, Dictionary<string, object>> table in tables) {
            MelonPreferences_Category category = Categories.FirstOrDefault(c => c.Identifier == table.Key && PathEquals(c.ResolvedFilePath, path));
            TomlTable output = new() {
                Name = table.Key,
                Comment = category != null && category.DisplayName != table.Key ? category.DisplayName : null,
            };
            foreach(KeyValuePair<string, object> pair in table.Value) {
                MelonPreferences_Entry entry = category?.GetEntry(pair.Key);
                output.Entries.Add(new TomlEntry { Key = pair.Key, Value = pair.Value, Comment = entry?.Comment });
            }
            yield return output;
        }
    }

    internal static void ApplyStoredValue(MelonPreferences_Category category, MelonPreferences_Entry entry) {
        // Nothing has been read off disk yet, so there is nothing to apply — and
        // resolving the default path would drag in the game directory for no
        // reason.
        if(stored.Count == 0) return;
        if(!stored.TryGetValue(category.ResolvedFilePath, out Dictionary<string, Dictionary<string, object>> tables)) return;
        if(!tables.TryGetValue(category.Identifier, out Dictionary<string, object> table)) return;
        if(!table.TryGetValue(entry.Identifier, out object raw)) return;
        try {
            entry.LoadRaw(raw);
        } catch(Exception e) {
            MelonLogger.Warning(entry.GetExceptionMessage($"could not be read from the config file: {e.Message}"));
        }
    }

    public static void RemoveCategoryFromFile(string filePath, string categoryName) {
        if(stored.TryGetValue(filePath, out Dictionary<string, Dictionary<string, object>> tables) && tables.Remove(categoryName))
            SaveFile(filePath, false);
    }

    private static bool PathEquals(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
