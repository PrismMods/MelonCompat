using System.Linq;
using MelonLoader.Utils;

namespace MelonLoader;

// The pre-0.5 loading API. Mods still call it to enumerate their peers, so it is
// a view over the same registry MelonAssembly fills.
public static class MelonHandler {
    public static string ModsDirectory => MelonEnvironment.ModsDirectory;
    public static string PluginsDirectory => MelonEnvironment.PluginsDirectory;

    public static List<MelonMod> Mods => MelonBase.RegisteredMelons.OfType<MelonMod>().ToList();
    public static List<MelonPlugin> Plugins => MelonBase.RegisteredMelons.OfType<MelonPlugin>().ToList();

    public static void LoadFromFile(string filepath, string symbolspath = null) => MelonAssembly.LoadMelonAssembly(filepath);
    public static void LoadFromFile(string filelocation, bool is_plugin) => MelonAssembly.LoadMelonAssembly(filelocation);

    public static void LoadFromByteArray(byte[] filedata, byte[] symbolsdata = null, string filepath = null) =>
        MelonAssembly.LoadRawMelonAssembly(filepath, filedata, symbolsdata);

    public static void LoadFromByteArray(byte[] filedata, string filelocation) => LoadFromByteArray(filedata, null, filelocation);
    public static void LoadFromByteArray(byte[] filedata, string filelocation, bool is_plugin) => LoadFromByteArray(filedata, null, filelocation);

    public static void LoadFromAssembly(System.Reflection.Assembly asm, string filepath = null) =>
        MelonAssembly.LoadMelonAssembly(filepath, asm);

    public static void LoadFromAssembly(System.Reflection.Assembly asm, string filelocation, bool is_plugin) =>
        LoadFromAssembly(asm, filelocation);

    public static string GetMelonHash(MelonBase melonBase) => melonBase?.Hash;

    public static bool IsMelonLoaded(string name) => MelonBase.RegisteredMelons.Any(m => m.Info?.Name == name);
    public static MelonBase GetMelon(string name) => MelonBase.RegisteredMelons.FirstOrDefault(m => m.Info?.Name == name);
}
