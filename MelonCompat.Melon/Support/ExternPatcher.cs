using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using dnlib.DotNet;
using HarmonyLib;
using MelonLoader.Utils;

namespace MelonLoader.Support;

// Patching engine internal calls, by rewriting their callers instead.
//
// UnityEngine.Screen.width, UnityEngine.Time.deltaTime and their neighbours are
// [MethodImpl(InternalCall)] — no managed body. Harmony patches a method by
// building a wrapper around its IL, so for these there is nothing to build from
// and the patch fails outright. Mods that rely on them (a renderer forcing an
// output resolution, a recorder forcing a frame time) then run half-patched, which
// is worse than not loading at all.
//
// The way around it is to leave the property alone and rewrite everyone who reads
// it: every `call Screen::get_width` in the game and mod assemblies becomes a call
// to a stand-in that runs the mod's patches. The mod is unchanged and unaware.
//
// The limits are deliberate. Only static, parameterless internal calls are handled
// — property getters, in practice — and only prefixes shaped `bool (ref T)` and
// postfixes shaped `void (ref T)`. Anything else is reported rather than guessed at.
public static class ExternPatcher {
    private sealed class Redirect {
        public int Id;
        public MethodInfo Dispatch;
    }

    private static readonly Dictionary<MethodBase, Redirect> Redirects = [];
    private static readonly Dictionary<MethodBase, int> Slots = [];
    private static readonly HashSet<Type> Registered = [];
    private static readonly HashSet<Assembly> Candidates = [];
    private static bool installed;

    public static int RedirectedTargets => Slots.Count;

    // True when this patch class targets something Harmony cannot patch and this
    // took responsibility for it.
    public static bool TryRegister(Type patchClass, MelonLogger.Instance logger) {
        MethodBase target;
        try {
            target = ResolveTarget(patchClass);
        } catch {
            return false;
        }
        if(target == null || !IsExtern(target)) return false;

        // Re-enabling a mod runs HarmonyInit again; its patches must not stack up.
        if(!Registered.Add(patchClass)) return true;

        Type valueType = (target as MethodInfo)?.ReturnType;
        if(target.IsStatic == false || valueType == null || valueType == typeof(void) || target.GetParameters().Length > 0) {
            logger?.Warning($"{patchClass.FullName} patches {Describe(target)}, an engine internal call that MelonCompat can only redirect when it is a static parameterless getter — skipped.");
            return true;
        }

        MethodInfo[] prefixes = PatchMethods(patchClass, "Prefix", typeof(HarmonyPrefix));
        MethodInfo[] postfixes = PatchMethods(patchClass, "Postfix", typeof(HarmonyPostfix));
        if(prefixes.Length == 0 && postfixes.Length == 0) return false;

        // Whoever patched an internal call almost certainly reads it too, so its
        // own assembly joins the set that gets scanned for call sites.
        Candidates.Add(patchClass.Assembly);

        if(!Slots.TryGetValue(target, out int id)) {
            id = ExternDispatch.CreateSlot(valueType, target);
            Slots[target] = id;
            Redirects[target] = new Redirect { Id = id, Dispatch = ExternDispatch.DispatchMethod(valueType) };
        }

        int added = 0;
        foreach(MethodInfo prefix in prefixes) added += TryAdd(id, valueType, prefix, true, patchClass, logger) ? 1 : 0;
        foreach(MethodInfo postfix in postfixes) added += TryAdd(id, valueType, postfix, false, patchClass, logger) ? 1 : 0;

        if(added > 0)
            logger?.Msg($"{Describe(target)} cannot be patched directly, so its call sites will be redirected instead ({patchClass.Name})");
        return true;
    }

    private static bool TryAdd(int id, Type valueType, MethodInfo patch, bool isPrefix, Type patchClass, MelonLogger.Instance logger) {
        try {
            ExternDispatch.AddPatch(id, valueType, patch, isPrefix);
            return true;
        } catch(Exception) {
            string wanted = isPrefix ? $"bool {patch.Name}(ref {valueType.Name} __result)" : $"void {patch.Name}(ref {valueType.Name} __result)";
            logger?.Warning($"{patchClass.FullName}.{patch.Name} is not a shape MelonCompat can redirect — it must be `static {wanted}`. That patch was skipped.");
            return false;
        }
    }

    // Done once, after every mod has registered, so the assemblies are scanned a
    // single time no matter how many mods want redirects.
    public static void InstallRedirects(Harmony harmony) {
        if(installed || Redirects.Count == 0) return;
        installed = true;

        MethodInfo transpiler = typeof(ExternPatcher).GetMethod(nameof(RedirectTranspiler), BindingFlags.NonPublic | BindingFlags.Static);
        int patched = 0, failed = 0;
        // Worth reporting: this rewrites every reader of the patched properties,
        // which for something like Time.deltaTime is a few hundred methods.
        System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

        int scanned = 0;
        foreach(Assembly assembly in ScanTargets()) {
            scanned++;
            foreach(MethodBase caller in FindCallers(assembly)) {
                try {
                    harmony.Patch(caller, transpiler: new HarmonyMethod(transpiler));
                    patched++;
                } catch(Exception e) {
                    failed++;
                    MelonLogger.Warning($"[Harmony] could not redirect the call sites in {caller.DeclaringType?.Name}.{caller.Name}: {e.Message}");
                }
            }
        }

        MelonLogger.Msg($"[Harmony] redirected {Slots.Count} engine {MelonUtils.MakePlural("call", Slots.Count)} across {patched} {MelonUtils.MakePlural("method", patched)} in {scanned} assemblies in {clock.ElapsedMilliseconds} ms"
            + (failed > 0 ? $" ({failed} could not be rewritten)" : ""));
    }

    // Every managed assembly in the process except the runtime's own and this
    // layer's own plumbing.
    //
    // Narrowing this to the game assemblies was wrong: UnityEngine.UI sizes
    // CanvasScaler from Screen.width, and the game's helper libraries read it too,
    // so a mod forcing a render resolution would still see stale layout. Rewriting
    // every managed reader is what makes this equivalent to detouring the property
    // itself — a native detour would reach exactly the same set, because C++
    // engine code never goes through the managed method either.
    private static readonly string[] SkippedPrefixes = [
        "System", "mscorlib", "netstandard", "Mono.", "I18N", "Microsoft.",
        "0Harmony", "dnlib", "UnityModManager", "MonoMod", "MelonLoader",
    ];

    private static IEnumerable<Assembly> ScanTargets() {
        List<Assembly> assemblies = [];
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            string name = assembly.GetName().Name;
            if(SkippedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal))) continue;
            if(assembly == typeof(ExternPatcher).Assembly) continue;
            assemblies.Add(assembly);
        }
        foreach(Assembly candidate in Candidates)
            if(!assemblies.Contains(candidate)) assemblies.Add(candidate);
        return assemblies;
    }

    // Parsing forty assemblies to find a handful of call sites is mostly wasted
    // work. A method name that is referenced at all appears verbatim in the
    // metadata string heap, so a raw byte scan rules most files out for the cost of
    // reading them.
    private static bool MightContainCallSites(byte[] data, IEnumerable<string> names) {
        foreach(string name in names) {
            byte[] needle = System.Text.Encoding.ASCII.GetBytes(name);
            for(int i = 0; i <= data.Length - needle.Length; i++) {
                int j = 0;
                while(j < needle.Length && data[i + j] == needle[j]) j++;
                if(j == needle.Length) return true;
            }
        }
        return false;
    }

    // The IL is read from the file with dnlib, because reflection cannot show which
    // methods contain which calls. Matches are then resolved back to runtime
    // methods by name and signature rather than by metadata token — a mod assembly
    // is rewritten before it is loaded, so its tokens need not match the file's.
    private static IEnumerable<MethodBase> FindCallers(Assembly assembly) {
        string path = LocationOf(assembly);
        if(string.IsNullOrEmpty(path) || !File.Exists(path)) yield break;

        HashSet<string> wanted = new(Redirects.Keys.Select(Key), StringComparer.Ordinal);
        byte[] data;
        try {
            data = File.ReadAllBytes(path);
        } catch {
            yield break;
        }
        if(!MightContainCallSites(data, Redirects.Keys.Select(m => m.Name).Distinct())) yield break;

        ModuleDefMD module = null;
        List<(string Type, string Method, int Params)> hits = [];
        try {
            module = ModuleDefMD.Load(data);
            foreach(TypeDef type in module.GetTypes()) {
                foreach(MethodDef method in type.Methods) {
                    if(method.Body == null) continue;
                    bool calls = method.Body.Instructions.Any(i =>
                        i.Operand is IMethod called && wanted.Contains(Key(called)));
                    if(calls) hits.Add((type.FullName, method.Name, method.MethodSig?.Params.Count ?? 0));
                }
            }
        } catch(Exception e) {
            MelonLogger.Warning($"[Harmony] could not scan {Path.GetFileName(path)} for call sites: {e.Message}");
            yield break;
        } finally {
            module?.Dispose();
        }

        foreach((string typeName, string methodName, int paramCount) in hits) {
            MethodBase resolved = null;
            try {
                Type declaring = assembly.GetType(typeName.Replace('/', '+'), throwOnError: false);
                resolved = declaring?
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == paramCount && !m.IsAbstract && !m.ContainsGenericParameters);
            } catch {
                resolved = null;
            }
            if(resolved != null) yield return resolved;
        }
    }

    private static string LocationOf(Assembly assembly) {
        try {
            if(!string.IsNullOrEmpty(assembly.Location)) return assembly.Location;
        } catch {
            // Assemblies loaded from memory can throw rather than return empty.
        }

        // A mod that was rewritten on load has no Location of its own.
        string origin = HarmonyBridge.OriginOf(assembly);
        if(!string.IsNullOrEmpty(origin)) return origin;

        MelonAssembly melon = MelonAssembly.LoadedAssemblies.FirstOrDefault(a => a.Assembly == assembly);
        if(melon != null && !string.IsNullOrEmpty(melon.Location)) return melon.Location;

        try {
            string managed = Path.Combine(MelonEnvironment.UnityGameManagedDirectory, assembly.GetName().Name + ".dll");
            return File.Exists(managed) ? managed : null;
        } catch {
            // Resolving the game directory needs Unity, which is not always there.
            return null;
        }
    }

    private static string Key(MethodBase method) => $"{method.DeclaringType?.FullName}::{method.Name}";
    private static string Key(IMethod method) => $"{method.DeclaringType?.FullName}::{method.Name}";

    // Swap the engine call for the stand-in. The id is pushed first, so the stack
    // ends up exactly as the original call left it: one value of the same type.
    private static IEnumerable<CodeInstruction> RedirectTranspiler(IEnumerable<CodeInstruction> instructions) {
        foreach(CodeInstruction instruction in instructions) {
            if(instruction.operand is MethodInfo called
               && (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
               && Redirects.TryGetValue(called, out Redirect redirect)) {
                // The call instruction is rewritten in place rather than replaced,
                // so the labels and try/catch bounds attached to it stay attached —
                // a branch that targeted this call still lands on it.
                instruction.opcode = OpCodes.Ldc_I4;
                instruction.operand = redirect.Id;
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Call, redirect.Dispatch);
                continue;
            }
            yield return instruction;
        }
    }

    public static bool IsExtern(MethodBase method) {
        System.Reflection.MethodImplAttributes flags = method.GetMethodImplementationFlags();
        return (flags & System.Reflection.MethodImplAttributes.InternalCall) != 0
            || (flags & System.Reflection.MethodImplAttributes.Native) != 0;
    }

    // Reads the [HarmonyPatch] attributes the same way Harmony does, then resolves
    // what they point at.
    private static MethodBase ResolveTarget(Type patchClass) {
        Type declaringType = null;
        string methodName = null;
        MethodType methodType = MethodType.Normal;
        Type[] argumentTypes = null;

        foreach(HarmonyAttribute attribute in patchClass.GetCustomAttributes(true).OfType<HarmonyAttribute>()) {
            HarmonyMethod info = attribute.info;
            if(info == null) continue;
            declaringType ??= info.declaringType;
            methodName ??= info.methodName;
            argumentTypes ??= info.argumentTypes;
            if(info.methodType.HasValue) methodType = info.methodType.Value;
        }

        if(declaringType == null || string.IsNullOrEmpty(methodName)) return null;

        return methodType switch {
            MethodType.Getter => AccessTools.DeclaredProperty(declaringType, methodName)?.GetGetMethod(true),
            MethodType.Setter => AccessTools.DeclaredProperty(declaringType, methodName)?.GetSetMethod(true),
            MethodType.Normal => AccessTools.DeclaredMethod(declaringType, methodName, argumentTypes),
            _ => null,
        };
    }

    private static MethodInfo[] PatchMethods(Type patchClass, string name, Type attribute) =>
        patchClass
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == name || m.GetCustomAttributes(attribute, false).Length > 0)
            .ToArray();

    private static string Describe(MethodBase method) => $"{method.DeclaringType?.FullName}.{method.Name}";
}
