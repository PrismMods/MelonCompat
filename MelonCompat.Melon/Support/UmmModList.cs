using System.IO;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;

namespace MelonLoader.Support;

// MelonCompat is plumbing, not a mod anyone configures, so it takes itself out of
// UnityModManager's list and puts the MelonLoader mods there instead — one row per
// mod, with its real name, author and version, and a toggle that works.
//
// The entries are synthetic: UnityModManager normally builds a ModEntry by reading
// an Info.json and loading an assembly, and neither happened for these. Leaving
// AssemblyName and EntryMethod empty is what keeps it from trying to load them,
// and marking them already-started is what keeps the toggle from triggering a load.
public static class UmmModList {
    private const string PreferencesCategory = "MelonCompat";

    private static readonly Dictionary<MelonBase, UnityModManager.ModEntry> published = [];
    private static readonly Dictionary<MelonBase, MelonPreferences_Entry<bool>> enabledPreferences = [];

    public static event Action Changed;

    public static bool IsEnabled(MelonBase melon) =>
        !enabledPreferences.TryGetValue(melon, out MelonPreferences_Entry<bool> entry) || entry.Value;

    // Called once UnityModManager has finished its own load pass. Doing this during
    // load would mutate the list it is iterating.
    public static void Publish(UnityModManager.ModEntry self, IEnumerable<MelonBase> melons) {
        Hide(self);

        MelonPreferences_Category category = MelonPreferences.CreateCategory(PreferencesCategory, "MelonCompat");
        category.IsHidden = true;

        foreach(MelonBase melon in melons) {
            try {
                Publish(melon, category, self.Path);
            } catch(Exception e) {
                MelonLogger.Warning($"could not add {melon.Info?.Name} to the UnityModManager list: {e.Message}");
            }
        }
        Changed?.Invoke();
    }

    private static void Publish(MelonBase melon, MelonPreferences_Category category, string fallbackPath) {
        string id = melon.ID ?? melon.Info?.Name ?? melon.GetType().FullName;
        MelonPreferences_Entry<bool> preference = category.CreateEntry(id, true, melon.Info?.Name ?? id, null, true);
        enabledPreferences[melon] = preference;

        UnityModManager.ModInfo info = new() {
            Id = id,
            DisplayName = melon.Info?.Name ?? id,
            Author = melon.Info?.Author,
            Version = melon.Info?.Version,
            // Left empty on purpose: this is what makes HasAssembly false, so
            // UnityModManager never tries to load an assembly for this row.
            AssemblyName = null,
            EntryMethod = null,
            HomePage = melon.Info?.DownloadLink,
        };

        string path = string.IsNullOrEmpty(melon.Location) ? fallbackPath : Path.GetDirectoryName(melon.Location);
        UnityModManager.ModEntry entry = new(info, path + Path.DirectorySeparatorChar);

        // Present the row as an already-loaded, already-started mod. Without this,
        // flipping the toggle sends UnityModManager off to load a mod that is
        // already in memory.
        SetField(entry, "mStarted", true);
        SetField(entry, "mErrorOnLoading", false);
        SetField(entry, "mFirstLoading", false);
        SetField(entry, "mActive", preference.Value);
        entry.Enabled = preference.Value;
        entry.OnToggle = (_, value) => Toggle(melon, preference, value);

        published[melon] = entry;
        Insert(entry);
    }

    // UnityModManager renders modEntries in list order, so slot the row where its
    // name belongs instead of appending it after every existing mod.
    private static void Insert(UnityModManager.ModEntry entry) {
        int index = UnityModManager.modEntries.FindIndex(
            other => string.Compare(NameOf(other), NameOf(entry), StringComparison.OrdinalIgnoreCase) > 0);
        if(index < 0) UnityModManager.modEntries.Add(entry);
        else UnityModManager.modEntries.Insert(index, entry);
    }

    private static string NameOf(UnityModManager.ModEntry entry) =>
        string.IsNullOrEmpty(entry.Info?.DisplayName) ? entry.Info?.Id ?? "" : entry.Info.DisplayName;

    // A MelonLoader mod has no disabled state, so the toggle is expressed as the
    // closest honest equivalent: deinitialise and drop its Harmony patches on the
    // way down, initialise again on the way up. Mods that cannot survive that were
    // already going to break on a UnityModManager reload.
    private static bool Toggle(MelonBase melon, MelonPreferences_Entry<bool> preference, bool value) {
        if(preference.Value == value) return true;
        preference.Value = value;

        try {
            if(value) {
                // Patches first, same order as startup — the mod's initialize
                // callback is entitled to assume they are already applied.
                melon.HarmonyInit();
                melon.OnInitializeMelon();
                melon.OnLateInitializeMelon();
                melon.LoggerInstance?.Msg("enabled");
            } else {
                melon.OnDeinitializeMelon();
                HarmonyBridge.UnpatchOwn(melon.HarmonyInstance, melon.LoggerInstance);
                melon.LoggerInstance?.Msg("disabled");
            }
        } catch(Exception e) {
            melon.LoggerInstance?.Error($"failed to {(value ? "enable" : "disable")}: {e}");
        }

        MelonPreferences.Save();
        Changed?.Invoke();
        return true;
    }

    public static void Withdraw() {
        foreach(UnityModManager.ModEntry entry in published.Values) UnityModManager.modEntries.Remove(entry);
        published.Clear();
        enabledPreferences.Clear();
    }

    private static void Hide(UnityModManager.ModEntry self) => UnityModManager.modEntries.Remove(self);

    private static void SetField(object target, string name, object value) =>
        target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
}
