using System;
using HarmonyLib;
using HarmonyLib.Tools;
using MelonLoader;

[assembly: MelonInfo(typeof(SampleMelonMod.SampleMod), "SampleMelonMod", "1.0.0", "MelonCompat")]
[assembly: MelonGame(null, null)]

namespace SampleMelonMod;

public class SampleMod : MelonMod {
    private MelonPreferences_Entry<bool> enabled;
    private MelonPreferences_Entry<int> amount;

    public override void OnInitializeMelon() {
        MelonPreferences_Category category = MelonPreferences.CreateCategory("SampleMelonMod", "Sample Mod");
        enabled = category.CreateEntry("Enabled", true, "Enabled");
        amount = category.CreateEntry("Amount", 5, "Amount");
        LoggerInstance.Msg($"initialised: enabled={enabled.Value} amount={amount.Value}");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName) =>
        LoggerInstance.Msg($"scene {buildIndex} '{sceneName}' loaded");

    // Every call in here exists only in HarmonyX. Under UnityModManager's Harmony
    // this method is what would throw MissingMethodException without translation.
    public static string TouchHarmonyX() {
        Logger.ChannelFilter = Logger.LogChannel.Warn;
        HarmonyFileLog.Enabled = false;

        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("melonCompat.sample");
        harmony.UnpatchSelf();
        HarmonyLib.Harmony.UnpatchID("nobody.at.all");
        return "ok";
    }

    // HarmonyX-only transpiler helpers: the Code opcode matchers, MatchForward's
    // bool overload, and the generic EmitDelegate. None of these exist in
    // UnityModManager's Harmony, so all three have to be translated.
    public static string TouchTranspilerHelpers() {
        CodeInstruction[] body = [
            new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0),
            new CodeInstruction(System.Reflection.Emit.OpCodes.Ret),
        ];

        CodeMatcher matcher = new CodeMatcher(body);
        matcher.MatchForward(false, Code.Ldarg_0);
        int position = matcher.Pos;

        CodeInstruction emitted = Transpilers.EmitDelegate<Func<int>>(() => 7);
        return position + "/" + (emitted != null ? "emitted" : "null");
    }

    // Exercises the MelonLoader surface that has no UnityModManager equivalent.
    public static string TouchMelonApi() {
        MelonLogger.Msg("hello from the sample mod");
        MelonLogger.Warning("a warning");
        MelonEvents.OnUpdate.Subscribe(() => { });
        return MelonUtils.MakePlural("mod", 2) + "/" + MelonLoader.Properties.BuildInfo.Version;
    }
}

// The two-argument string overload of HarmonyPatch is HarmonyX-only, so this
// attribute cannot even be constructed without the translation pass.
[HarmonyPatch("System.String", "Trim")]
[HarmonyWrapSafe]
public static class SamplePatch {
    public static void Postfix() { }
}

// A patch that actually does something, so the loader's automatic PatchAll can be
// proven rather than assumed: MelonLoader patches a mod's annotated classes for
// it, and mods are written expecting exactly that.
public static class PatchTarget {
    public static string Value() => "unpatched";
}

[HarmonyPatch(typeof(PatchTarget), nameof(PatchTarget.Value))]
public static class PatchTargetPatch {
    public static void Postfix(ref string __result) => __result = "patched";
}

// An engine-style internal call: public, static, parameterless, no managed body —
// exactly the shape of UnityEngine.Screen.width. Harmony cannot patch it, so
// MelonCompat has to redirect the call sites instead.
public static class ExternCaller {
    public static int Read() => System.Environment.ProcessorCount;
}

[HarmonyPatch(typeof(System.Environment), "ProcessorCount", MethodType.Getter)]
public static class ProcessorCountPatch {
    public static bool Prefix(ref int __result) {
        __result = 9999;
        return false;
    }
}
