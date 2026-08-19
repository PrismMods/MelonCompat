namespace MelonLoader;

// The global event bus. Startup events are one-time-use so a mod that subscribes
// after the game has already started still gets its callback instead of waiting
// forever for an event that has been and gone.
public static class MelonEvents {
    public static readonly MelonEvent OnPreInitialization = new(true);
    public static readonly MelonEvent OnApplicationEarlyStart = new(true);
    public static readonly MelonEvent OnPreModsLoaded = new(true);

    // Internal in MelonLoader too. This is the phase where every registered melon
    // applies the [HarmonyPatch] classes in its own assembly; it runs before
    // OnApplicationStart so a mod's OnInitializeMelon can assume its patches are
    // already in place. One-time-use, so a melon registered later patches
    // immediately on subscribe instead of waiting for an event that already fired.
    internal static readonly MelonEvent MelonHarmonyInit = new(true);
    public static readonly MelonEvent OnPreSupportModule = new(true);
    public static readonly MelonEvent OnApplicationStart = new(true);
    public static readonly MelonEvent OnApplicationLateStart = new(true);
    public static readonly MelonEvent OnApplicationQuit = new();
    public static readonly MelonEvent OnApplicationDefiniteQuit = new(true);
    public static readonly MelonEvent OnUpdate = new();
    public static readonly MelonEvent OnFixedUpdate = new();
    public static readonly MelonEvent OnLateUpdate = new();
    public static readonly MelonEvent OnGUI = new();
    public static readonly MelonEvent<int, string> OnSceneWasLoaded = new();
    public static readonly MelonEvent<int, string> OnSceneWasInitialized = new();
    public static readonly MelonEvent<int, string> OnSceneWasUnloaded = new();
}
