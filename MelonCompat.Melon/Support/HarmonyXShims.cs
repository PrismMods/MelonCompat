using System.Reflection;
using HarmonyLib;

namespace MelonLoader.Support;

// HarmonyX methods that UnityModManager's Harmony does not have, expressed in
// terms of the ones it does. The translator rewrites calls in mod assemblies to
// land here, so the mod's own IL never has to be correct for pardeike Harmony —
// it only has to be correct for HarmonyX, which it already is.
public static class HarmonyXCompat {
    // HarmonyX splits unpatching three ways: UnpatchSelf() on the instance for
    // "my own patches", and two statics for "somebody else's" and "everything".
    // pardeike spells all three as the instance UnpatchAll(id), where a null id
    // means every patch in the process — so the id has to be passed deliberately.
    public static void UnpatchSelf(Harmony harmony) => harmony?.UnpatchAll(harmony.Id);

    public static void UnpatchID(string harmonyID) => Scratch().UnpatchAll(harmonyID);

    // The argument-less HarmonyX UnpatchAll really does mean everything, other
    // mods included. It is faithful, and loud, because it is rarely what a mod
    // actually wants.
    public static void UnpatchAll() {
        MelonLogger.Warning("[Harmony] a mod called Harmony.UnpatchAll(), which removes every Harmony patch in the process — including other mods'.");
        Scratch().UnpatchAll(null);
    }

    private static Harmony Scratch() => new("meloncompat.unpatch");

    public static void PatchAll(Harmony harmony, Type type) => harmony?.CreateClassProcessor(type).Patch();

    public static Harmony CreateAndPatchAll(Type type, string harmonyInstanceId = null) {
        Harmony harmony = new(harmonyInstanceId ?? $"harmonyauto.{Guid.NewGuid()}");
        PatchAll(harmony, type);
        return harmony;
    }

    public static Harmony CreateAndPatchAll(Assembly assembly, string harmonyInstanceId = null) {
        Harmony harmony = new(harmonyInstanceId ?? $"harmonyauto.{Guid.NewGuid()}");
        harmony.PatchAll(assembly);
        return harmony;
    }

    public static PatchClassProcessor CreateClassProcessor(Harmony harmony, Type type, bool allowUnannotatedType) =>
        harmony.CreateClassProcessor(type);

    public static PatchClassProcessor NewPatchClassProcessor(Harmony harmony, Type type, bool allowUnannotatedType) =>
        new(harmony, type);

    // HarmonyX's Patch takes a sixth ilmanipulator argument. There is no
    // equivalent, so the other five are applied and the manipulator is reported.
    public static MethodInfo Patch(
        Harmony harmony,
        MethodBase original,
        HarmonyMethod prefix,
        HarmonyMethod postfix,
        HarmonyMethod transpiler,
        HarmonyMethod finalizer,
        HarmonyMethod ilmanipulator) {
        if(ilmanipulator != null) WarnUnsupported($"an IL manipulator on {Describe(original)}");
        return harmony.Patch(original, prefix, postfix, transpiler, finalizer);
    }

    public static MethodInfo ReversePatch(
        Harmony harmony,
        MethodBase original,
        HarmonyMethod standin,
        MethodInfo postTranspiler,
        MethodInfo postManipulator) {
        if(postManipulator != null) WarnUnsupported($"a reverse-patch IL manipulator on {Describe(original)}");
        return Harmony.ReversePatch(original, standin, postTranspiler);
    }

    public static PatchProcessor AddILManipulator(PatchProcessor processor, HarmonyMethod manipulator) {
        WarnUnsupported("PatchProcessor.AddILManipulator");
        return processor;
    }

    public static PatchProcessor AddILManipulator(PatchProcessor processor, MethodInfo manipulator) {
        WarnUnsupported("PatchProcessor.AddILManipulator");
        return processor;
    }

    // HarmonyX folds "match from the start or the end" into a bool; pardeike keeps
    // them as separate methods.
    public static CodeMatcher MatchForward(CodeMatcher matcher, bool useEnd, params CodeMatch[] matches) =>
        useEnd ? matcher.MatchEndForward(matches) : matcher.MatchStartForward(matches);

    public static CodeMatcher MatchBack(CodeMatcher matcher, bool useEnd, params CodeMatch[] matches) =>
        useEnd ? matcher.MatchEndBackwards(matches) : matcher.MatchStartBackwards(matches);

    // Same idea, different name: emit a call to an inline delegate.
    public static CodeInstruction EmitDelegate<T>(T action) where T : Delegate =>
        CodeInstruction.CallClosure(action);

    private static string Describe(MethodBase method) =>
        method == null ? "an unknown method" : $"{method.DeclaringType?.FullName}.{method.Name}";

    internal static void WarnUnsupported(string what) =>
        MelonLogger.Warning($"[Harmony] {what} is a HarmonyX feature that UnityModManager's Harmony cannot run — that patch was skipped.");
}
