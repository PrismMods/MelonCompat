using System.Linq;

namespace MelonLoader;

// Melon<MyMod>.Logger is how a mod reaches its own logger from static code, and
// Melon<MyMod>.Instance is how it reaches itself. Both are cached on first use,
// because the alternative — a registry scan — sits behind log calls that mods make
// from hot paths.
public static class Melon<T> where T : MelonBase {
    private static T instance;

    public static T Instance {
        get {
            if(instance != null && instance.Registered) return instance;
            return instance = MelonBase.RegisteredMelons.OfType<T>().FirstOrDefault();
        }
    }

    public static MelonLogger.Instance Logger => Instance?.LoggerInstance;
}
