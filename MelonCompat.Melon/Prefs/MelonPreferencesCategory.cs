using System.IO;
using System.Linq;
using MelonLoader.Preferences;

namespace MelonLoader;

// A TOML table's worth of preferences. Categories can point at their own file;
// by default they all share UserData/MelonPreferences.cfg, which is exactly where
// MelonLoader put them, so an existing config keeps working.
public class MelonPreferences_Category {
    public readonly List<MelonPreferences_Entry> Entries = [];

    public string Identifier { get; }
    public string DisplayName { get; set; }
    public bool IsHidden { get; set; }
    public bool IsInlined { get; set; }
    public bool ShouldSave { get; set; } = true;
    public string FilePath { get; private set; }

    internal string ResolvedFilePath => FilePath ?? MelonPreferences.DefaultFilePath;

    internal MelonPreferences_Category(string identifier, string displayName, bool isHidden = false, bool shouldSave = true) {
        Identifier = identifier;
        DisplayName = displayName ?? identifier;
        IsHidden = isHidden;
        ShouldSave = shouldSave;
    }

    public MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName, bool isHidden) =>
        CreateEntry(identifier, defaultValue, displayName, null, isHidden, false, null);

    public MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName = null, string description = null, bool isHidden = false, bool dontSaveDefault = false, ValueValidator validator = null) {
        if(string.IsNullOrEmpty(identifier)) throw new ArgumentException("a preference needs an identifier", nameof(identifier));

        MelonPreferences_Entry existing = GetEntry(identifier);
        if(existing != null) {
            // A mod re-creating an entry after a reload must get the live one back,
            // not a second entry that silently shadows the saved value.
            if(existing is MelonPreferences_Entry<T> typed) return typed;
            throw new InvalidOperationException(existing.GetExceptionMessage($"already exists as {existing.GetReflectedType()}, not {typeof(T)}"));
        }

        MelonPreferences_Entry<T> entry = new(identifier, defaultValue) {
            DisplayName = displayName ?? identifier,
            Description = description,
            Comment = description,
            IsHidden = isHidden,
            DontSaveDefault = dontSaveDefault,
            Validator = validator,
            Category = this,
        };
        if(validator != null) entry.Value = defaultValue;
        Entries.Add(entry);

        // The file may already hold a value for this entry — a mod that creates
        // its preferences late still gets what the user configured.
        MelonPreferences.ApplyStoredValue(this, entry);
        return entry;
    }

    public bool DeleteEntry(string identifier) {
        MelonPreferences_Entry entry = GetEntry(identifier);
        return entry != null && Entries.Remove(entry);
    }

    public bool RenameEntry(string identifier, string newIdentifier) {
        MelonPreferences_Entry entry = GetEntry(identifier);
        if(entry == null || HasEntry(newIdentifier)) return false;
        entry.Identifier = newIdentifier;
        return true;
    }

    public MelonPreferences_Entry GetEntry(string identifier) =>
        Entries.FirstOrDefault(e => e.Identifier == identifier);

    public MelonPreferences_Entry<T> GetEntry<T>(string identifier) => GetEntry(identifier) as MelonPreferences_Entry<T>;

    public bool HasEntry(string identifier) => GetEntry(identifier) != null;

    public void SetFilePath(string filepath) => SetFilePath(filepath, true, true);
    public void SetFilePath(string filepath, bool autoload) => SetFilePath(filepath, autoload, true);

    public void SetFilePath(string filepath, bool autoload, bool printmsg) {
        FilePath = filepath;
        if(autoload) LoadFromFile(printmsg);
    }

    public void ResetFilePath() {
        FilePath = null;
        LoadFromFile(false);
    }

    public void SaveToFile(bool printmsg = true) => MelonPreferences.SaveFile(ResolvedFilePath, printmsg);
    public void LoadFromFile(bool printmsg = true) => MelonPreferences.LoadFile(ResolvedFilePath, printmsg);

    // MelonLoader watches config files for external edits. Nothing here creates a
    // watcher, so there is nothing to destroy.
    public void DestroyFileWatcher() { }
}
