namespace MelonLoader;

// A mod. Scene callbacks are the part that does not exist under UnityModManager
// at all — the host subscribes to SceneManager and fans out to these.
public abstract class MelonMod : MelonTypeBase<MelonMod> {
    public override string MelonTypeName => "Mod";

    public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }
    public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }
    public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName) { }

    // Pre-0.3 scene callbacks.
    public virtual void OnLevelWasLoaded(int level) { }
    public virtual void OnLevelWasInitialized(int level) { }
}

// Plugins run before the game does under MelonLoader. UnityModManager starts
// after the engine is already up, so a plugin's early callbacks fire as soon as
// it is registered rather than actually early.
public abstract class MelonPlugin : MelonTypeBase<MelonPlugin> {
    public override string MelonTypeName => "Plugin";

    public virtual void OnPreInitialization() { }
    public virtual void OnApplicationEarlyStart() { }
    public virtual void OnPreModsLoaded() { }
    public virtual void OnApplicationStarted() { }
}
