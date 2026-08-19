using System;
using System.IO;
using System.Reflection;

// Runs under Mono, which is the runtime family the game actually uses, and proves
// the two things that cannot be proven by compiling: that a mod built against the
// real MelonLoader and HarmonyX loads against the shim, and that its HarmonyX-only
// calls survive translation onto UnityModManager's Harmony.
internal static class TranslationTest {
    private static Assembly shim;
    private static Assembly harmony;
    private static string directory;
    private static int failures;

    private static int Main(string[] args) {
        directory = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;

        shim = Assembly.LoadFrom(Path.Combine(directory, "MelonLoader.dll"));
        harmony = Assembly.LoadFrom(Path.Combine(directory, "0Harmony.dll"));
        Assembly.LoadFrom(Path.Combine(directory, "dnlib.dll"));

        BindLog();
        Check("Harmony is UnityModManager's, not HarmonyX's", harmony.GetName().Version.ToString(), "2.4.2.0");

        Assembly mod = Translate(Path.Combine(directory, "SampleMelonMod.dll"));
        Type sample = mod.GetType("SampleMelonMod.SampleMod", true);

        // The mod is compiled against HarmonyX and must end up on UnityModManager's
        // Harmony, translated.
        Check("mod runs on UnityModManager's Harmony", HarmonyBinding(mod), "0Harmony 2.4.2.0");

        // Each of these would throw MissingMethodException on an untranslated mod.
        Check("HarmonyX-only calls run", Invoke(sample, "TouchHarmonyX"), "ok");
        Check("MelonLoader API runs", Invoke(sample, "TouchMelonApi"), "mods/0.7.3");
        Check("HarmonyX transpiler helpers translate", Invoke(sample, "TouchTranspilerHelpers"), "0/emitted");
        Check("HarmonyX-only attribute constructs", AttributeCheck(mod), "HarmonyPatch+HarmonyWrapSafe");
        Check("automatic PatchAll applies the mod's patches", AutoPatch(mod), "unpatched->patched");
        Check("internal calls are patched by redirecting call sites", ExternRedirect(mod), "patched->9999");
        Check("preferences round-trip through TOML", PreferencesRoundTrip(), "False|42");

        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL ({failures})");
        return failures == 0 ? 0 : 1;
    }

    private static string HarmonyBinding(Assembly mod) {
        try {
            object attribute = mod.GetType("SampleMelonMod.PatchTargetPatch", true).GetCustomAttributes(false)[0];
            AssemblyName name = attribute.GetType().Assembly.GetName();
            return name.Name + " " + name.Version;
        } catch(Exception e) {
            return "threw: " + e;
        }
    }

    private static Assembly Translate(string path) {
        Type bridge = shim.GetType("MelonLoader.Support.HarmonyBridge", true);
        MethodInfo load = bridge.GetMethod("LoadTranslated", BindingFlags.Public | BindingFlags.Static);
        return (Assembly)load.Invoke(null, new object[] { path });
    }

    private static string Invoke(Type type, string name) {
        try {
            return (string)type.GetMethod(name, BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
        } catch(TargetInvocationException e) {
            return "threw: " + e.InnerException;
        }
    }

    // MelonLoader applies a mod's [HarmonyPatch] classes on its behalf. This runs
    // the same call the loader makes and checks the patch actually took effect.
    private static string AutoPatch(Assembly mod) {
        try {
            Type target = mod.GetType("SampleMelonMod.PatchTarget", true);
            MethodInfo value = target.GetMethod("Value", BindingFlags.Public | BindingFlags.Static);
            string before = (string)value.Invoke(null, null);

            Type utils = shim.GetType("MelonLoader.MelonUtils", true);
            Type harmonyType = harmony.GetType("HarmonyLib.Harmony", true);
            object instance = Activator.CreateInstance(harmonyType, "meloncompat.test.autopatch");
            MethodInfo patchAll = utils.GetMethod("TryPatchAll", new[] { harmonyType, typeof(Assembly) });
            patchAll.Invoke(null, new object[] { instance, mod });

            string after = (string)value.Invoke(null, null);
            harmonyType.GetMethod("UnpatchAll", new[] { typeof(string) })
                .Invoke(instance, new object[] { "meloncompat.test.autopatch" });
            return before + "->" + after;
        } catch(Exception e) {
            return "threw: " + e;
        }
    }

    // The case Harmony cannot do at all: a method with no managed body. The patch
    // is applied by rewriting whoever calls it, so the proof is that an unrelated
    // method's return value changes.
    private static string ExternRedirect(Assembly mod) {
        try {
            Type caller = mod.GetType("SampleMelonMod.ExternCaller", true);
            MethodInfo read = caller.GetMethod("Read", BindingFlags.Public | BindingFlags.Static);
            int before = (int)read.Invoke(null, null);

            Type patcher = shim.GetType("MelonLoader.Support.ExternPatcher", true);
            Type patchClass = mod.GetType("SampleMelonMod.ProcessorCountPatch", true);
            bool handled = (bool)patcher.GetMethod("TryRegister").Invoke(null, new object[] { patchClass, null });
            if(!handled) return "the patch class was not recognised as an internal call";

            Type harmonyType = harmony.GetType("HarmonyLib.Harmony", true);
            object instance = Activator.CreateInstance(harmonyType, "meloncompat.test.extern");
            patcher.GetMethod("InstallRedirects").Invoke(null, new[] { instance });

            int after = (int)read.Invoke(null, null);
            return (before == after ? "unchanged" : "patched") + "->" + after;
        } catch(Exception e) {
            return "threw: " + e;
        }
    }

    private static string AttributeCheck(Assembly mod) {
        try {
            object[] attributes = mod.GetType("SampleMelonMod.SamplePatch", true).GetCustomAttributes(false);
            string names = "";
            foreach(object attribute in attributes)
                names += (names.Length == 0 ? "" : "+") + attribute.GetType().Name;
            return names;
        } catch(Exception e) {
            return "threw: " + e;
        }
    }

    // Exercises the config layer end to end: create, save, corrupt in memory,
    // reload from the file, and see the saved values come back.
    private static string PreferencesRoundTrip() {
        try {
            string path = Path.Combine(Path.GetTempPath(), "meloncompat-test-" + Guid.NewGuid().ToString("N") + ".cfg");
            Type preferences = shim.GetType("MelonLoader.MelonPreferences", true);
            object category = FindMethod(preferences, "CreateCategory", 0, 2).Invoke(null, new object[] { "TestCategory", "Test Category" });

            // The file path is set first so the category never has to fall back to
            // the game's UserData directory, which does not exist outside a game.
            category.GetType().GetMethod("SetFilePath", new[] { typeof(string), typeof(bool), typeof(bool) })
                .Invoke(category, new object[] { path, false, false });

            MethodInfo createEntry = FindMethod(category.GetType(), "CreateEntry", 1, 4);
            object flag = createEntry.MakeGenericMethod(typeof(bool)).Invoke(category, new object[] { "Flag", true, "Flag", false });
            object number = createEntry.MakeGenericMethod(typeof(int)).Invoke(category, new object[] { "Number", 7, "Number", false });

            SetValue(flag, false);
            SetValue(number, 42);
            category.GetType().GetMethod("SaveToFile").Invoke(category, new object[] { false });

            SetValue(flag, true);
            SetValue(number, 1);
            category.GetType().GetMethod("LoadFromFile").Invoke(category, new object[] { false });

            string result = GetValue(flag) + "|" + GetValue(number);
            File.Delete(path);
            return result;
        } catch(Exception e) {
            return "threw: " + e;
        }
    }

    // Overload resolution by shape, because several of these differ only in a
    // generic parameter that Type.GetMethod cannot express.
    private static MethodInfo FindMethod(Type type, string name, int genericArguments, int parameters) {
        foreach(MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) {
            if(method.Name != name) continue;
            if(method.GetGenericArguments().Length != genericArguments) continue;
            if(method.GetParameters().Length != parameters) continue;
            return method;
        }
        throw new MissingMethodException(type.FullName, name);
    }

    private static void SetValue(object entry, object value) =>
        entry.GetType().GetProperty("BoxedValue").SetValue(entry, value, null);

    private static string GetValue(object entry) =>
        entry.GetType().GetProperty("BoxedValue").GetValue(entry, null).ToString();

    private static void BindLog() {
        Type sink = shim.GetType("MelonLoader.Support.LogSink", true);
        Action<string> write = text => Console.WriteLine("  log: " + text);
        sink.GetMethod("Bind").Invoke(null, new object[] { write, write, write });
    }

    private static void Check(string what, string actual, string expected) {
        if(actual != null && actual.StartsWith("SKIP:", StringComparison.Ordinal)) {
            Console.WriteLine($"skip {what}");
            Console.WriteLine($"     {actual.Substring(5).Trim()}");
            return;
        }
        bool ok = actual == expected;
        if(!ok) failures++;
        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {what}");
        if(!ok) Console.WriteLine($"     expected: {expected}\n     actual:   {actual}");
    }

    // The mod was compiled against the real MelonLoader and HarmonyX; both names
    // have to answer with what is loaded here instead, which is the same trick the
    // bootstrap plays inside the game.
    private static Assembly Resolve(object sender, ResolveEventArgs args) {
        string name = new AssemblyName(args.Name).Name;
        if(name == "MelonLoader") return shim;
        if(name == "0Harmony") return harmony;
        string path = Path.Combine(directory, name + ".dll");
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }
}
