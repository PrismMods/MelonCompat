using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using HarmonyLib;

namespace MelonLoader.Support;

// Mods are compiled against HarmonyX; UnityModManager runs pardeike's Harmony.
// The two share an assembly name, a namespace and nearly all of their surface, so
// a mod's references resolve to UMM's Harmony on their own — except for the
// handful of members HarmonyX added, which would throw MissingMethodException the
// moment they were reached.
//
// This rewrites exactly those references, in the assembly's IL, on the way in.
// Everything else is left untouched, and an assembly with nothing to translate is
// loaded straight off disk so it keeps a real Assembly.Location.
//
// Shipping HarmonyX instead was tried and reverted: HarmonyX 2.10 rides on
// MonoMod 22.x, which cannot make memory executable on Apple Silicon (it uses
// plain mprotect where arm64 macOS needs MAP_JIT), so every patch failed. The
// MonoMod.Core embedded in UnityModManager's Harmony is new enough to handle it,
// which makes it the only working detour engine available here.
public static class HarmonyBridge {
    // HarmonyX member -> the compatibility method that expresses it in terms of
    // pardeike Harmony. Instance methods gain the instance as a first argument,
    // which is exactly what the IL already has on the stack.
    private static readonly Dictionary<string, string> MethodMap = new(StringComparer.Ordinal) {
        ["HarmonyLib.Harmony::UnpatchSelf()"] = nameof(HarmonyXCompat.UnpatchSelf),
        ["HarmonyLib.Harmony::UnpatchAll()"] = nameof(HarmonyXCompat.UnpatchAll),
        ["HarmonyLib.Harmony::UnpatchID(System.String)"] = nameof(HarmonyXCompat.UnpatchID),
        ["HarmonyLib.Harmony::PatchAll(System.Type)"] = nameof(HarmonyXCompat.PatchAll),
        ["HarmonyLib.Harmony::CreateClassProcessor(System.Type,System.Boolean)"] = nameof(HarmonyXCompat.CreateClassProcessor),
        ["HarmonyLib.Harmony::CreateAndPatchAll(System.Type,System.String)"] = nameof(HarmonyXCompat.CreateAndPatchAll),
        ["HarmonyLib.Harmony::CreateAndPatchAll(System.Reflection.Assembly,System.String)"] = nameof(HarmonyXCompat.CreateAndPatchAll),
        ["HarmonyLib.Harmony::Patch(System.Reflection.MethodBase,HarmonyLib.HarmonyMethod,HarmonyLib.HarmonyMethod,HarmonyLib.HarmonyMethod,HarmonyLib.HarmonyMethod,HarmonyLib.HarmonyMethod)"] = nameof(HarmonyXCompat.Patch),
        ["HarmonyLib.Harmony::ReversePatch(System.Reflection.MethodBase,HarmonyLib.HarmonyMethod,System.Reflection.MethodInfo,System.Reflection.MethodInfo)"] = nameof(HarmonyXCompat.ReversePatch),
        ["HarmonyLib.PatchProcessor::AddILManipulator(HarmonyLib.HarmonyMethod)"] = nameof(HarmonyXCompat.AddILManipulator),
        ["HarmonyLib.PatchProcessor::AddILManipulator(System.Reflection.MethodInfo)"] = nameof(HarmonyXCompat.AddILManipulator),
        ["HarmonyLib.PatchClassProcessor::.ctor(HarmonyLib.Harmony,System.Type,System.Boolean)"] = nameof(HarmonyXCompat.NewPatchClassProcessor),
        ["HarmonyLib.CodeMatcher::MatchForward(System.Boolean,HarmonyLib.CodeMatch[])"] = nameof(HarmonyXCompat.MatchForward),
        ["HarmonyLib.CodeMatcher::MatchBack(System.Boolean,HarmonyLib.CodeMatch[])"] = nameof(HarmonyXCompat.MatchBack),
        ["HarmonyLib.Transpilers::EmitDelegate(!!0)"] = nameof(HarmonyXCompat.EmitDelegate),
    };

    // Types HarmonyX defines and pardeike Harmony does not. Repointing the
    // reference to this assembly is enough to let the mod load.
    private static readonly HashSet<string> ShimmedTypes = new(StringComparer.Ordinal) {
        "HarmonyLib.HarmonyILManipulator",
        "HarmonyLib.HarmonyWrapSafe",
        "HarmonyLib.HarmonyEmitIL",
        "HarmonyLib.HarmonyGlobalSettings",
        "HarmonyLib.InlineSignature",
        "HarmonyLib.MemberNotFoundException",
        "HarmonyLib.InvalidHarmonyPatchArgumentException",
        "HarmonyLib.Tools.Logger",
        "HarmonyLib.Tools.Logger/LogChannel",
        "HarmonyLib.Tools.Logger/LogEventArgs",
        "HarmonyLib.Tools.HarmonyFileLog",
    };

    // Code and its 227 nested opcode matchers, which are all shimmed together.
    private const string ShimmedTypePrefix = "HarmonyLib.Code/";

    // HarmonyX-only debug knobs on HarmonyMethod and Patch. Assignments to them
    // are dropped; there is nothing on the other side to assign.
    private static readonly HashSet<string> DroppedFields = new(StringComparer.Ordinal) {
        "HarmonyLib.HarmonyMethod::debugEmitPath",
        "HarmonyLib.HarmonyMethod::wrapTryCatch",
        "HarmonyLib.Patch::debugEmitPath",
        "HarmonyLib.Patch::wrapTryCatch",
    };

    private static readonly string[] UntranslatableTypePrefixes = [
        "HarmonyLib.Public.Patching.",
        "HarmonyLib.Internal.",
    ];

    private const string HarmonyAssemblyName = "0Harmony";

    // An assembly loaded from rewritten bytes has no Location, so where it came
    // from is remembered here. Anything that needs to read a mod's IL back off
    // disk — the call-site scanner, for one — would otherwise come up empty.
    private static readonly Dictionary<Assembly, string> Origins = [];

    public static string OriginOf(Assembly assembly) {
        lock(Origins) return assembly != null && Origins.TryGetValue(assembly, out string path) ? path : null;
    }

    public static Assembly LoadTranslated(string path) {
        string full = Path.GetFullPath(path);
        byte[] original = File.ReadAllBytes(full);
        byte[] translated = Translate(original, Path.GetFileName(full));
        Assembly loaded = translated == null ? Assembly.LoadFrom(full) : Assembly.Load(translated);
        lock(Origins) Origins[loaded] = full;
        return loaded;
    }

    private static byte[] Translate(byte[] data, string displayName) {
        ModuleDefMD module = null;
        try {
            module = ModuleDefMD.Load(data);
            if(!ReferencesHarmony(module)) return null;

            int changes = 0;
            Importer importer = new(module);
            AssemblyRef shimAssembly = module.UpdateRowId(new AssemblyRefUser(typeof(HarmonyXCompat).Assembly.GetName()));

            changes += RetargetTypes(module, shimAssembly, displayName);
            changes += RetargetCalls(module, importer);
            changes += DropUnsupportedFieldWrites(module);
            changes += RetargetAttributes(module, importer);

            if(changes == 0) return null;

            MelonLogger.Msg($"[Harmony] translated {changes} HarmonyX {MelonUtils.MakePlural("reference", changes)} in {displayName}");

            using MemoryStream output = new();
            module.Write(output, new ModuleWriterOptions(module) {
                MetadataOptions = { Flags = MetadataFlags.PreserveAll },
            });
            return output.ToArray();
        } catch(Exception e) {
            MelonLogger.Warning($"[Harmony] could not translate {displayName} ({e.Message}) — loading it unmodified");
            return null;
        } finally {
            module?.Dispose();
        }
    }

    private static bool ReferencesHarmony(ModuleDefMD module) =>
        module.GetAssemblyRefs().Any(a => string.Equals(a.Name, HarmonyAssemblyName, StringComparison.OrdinalIgnoreCase));

    private static int RetargetTypes(ModuleDefMD module, AssemblyRef shimAssembly, string displayName) {
        int changes = 0;
        HashSet<string> reported = [];
        foreach(TypeRef typeRef in module.GetTypeRefs().ToArray()) {
            if(!IsHarmonyScope(typeRef.ResolutionScope)) continue;
            string fullName = typeRef.FullName;

            if(ShimmedTypes.Contains(fullName) || fullName == "HarmonyLib.Code" || fullName.StartsWith(ShimmedTypePrefix, StringComparison.Ordinal)) {
                typeRef.ResolutionScope = shimAssembly;
                changes++;
                if(fullName == "HarmonyLib.HarmonyILManipulator" && reported.Add(fullName))
                    HarmonyXCompat.WarnUnsupported($"the ILManipulator patches in {displayName}");
                continue;
            }

            if(UntranslatableTypePrefixes.Any(prefix => fullName.StartsWith(prefix, StringComparison.Ordinal)) && reported.Add(fullName))
                MelonLogger.Warning($"[Harmony] {displayName} uses {fullName}, a HarmonyX internal with no counterpart in UnityModManager's Harmony — that code path will fail if it runs.");
        }
        return changes;
    }

    private static int RetargetCalls(ModuleDefMD module, Importer importer) {
        int changes = 0;
        foreach(MethodDef method in module.GetTypes().SelectMany(t => t.Methods)) {
            if(method.Body == null) continue;
            foreach(Instruction instruction in method.Body.Instructions) {
                if(instruction.Operand is not IMethod target) continue;

                // A generic call is a MethodSpec wrapping the definition; the map is
                // keyed on the definition, and the instantiation has to be carried
                // over to the replacement.
                MethodSpec spec = target as MethodSpec;
                IMethod definition = spec?.Method ?? target;
                if(!IsHarmonyMember(definition)) continue;
                if(!MethodMap.TryGetValue(Signature(definition), out string replacementName)) continue;

                MethodInfo replacement = FindShim(replacementName, definition);
                if(replacement == null) continue;

                IMethod imported = importer.Import(replacement);
                if(spec != null && imported is IMethodDefOrRef generic)
                    imported = module.UpdateRowId(new MethodSpecUser(generic, spec.GenericInstMethodSig));

                instruction.OpCode = OpCodes.Call;
                instruction.Operand = imported;
                changes++;
            }
        }
        return changes;
    }

    private static MethodInfo FindShim(string name, IMethod target) {
        MethodInfo[] candidates = typeof(HarmonyXCompat)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == name)
            .ToArray();
        if(candidates.Length == 1) return candidates[0];

        string[] wanted = target.MethodSig.Params.Select(p => p.FullName).ToArray();
        foreach(MethodInfo candidate in candidates) {
            ParameterInfo[] parameters = candidate.GetParameters();
            int offset = parameters.Length - wanted.Length;
            if(offset < 0) continue;
            if(wanted.Where((t, i) => parameters[i + offset].ParameterType.FullName != t).Any()) continue;
            return candidate;
        }
        return candidates.FirstOrDefault();
    }

    private static int DropUnsupportedFieldWrites(ModuleDefMD module) {
        int changes = 0;
        foreach(MethodDef method in module.GetTypes().SelectMany(t => t.Methods)) {
            if(method.Body == null) continue;
            IList<Instruction> instructions = method.Body.Instructions;
            for(int i = 0; i < instructions.Count; i++) {
                Instruction instruction = instructions[i];
                if(instruction.Operand is not IField field || !IsHarmonyMember(field)) continue;
                if(!DroppedFields.Contains($"{field.DeclaringType.FullName}::{field.Name}")) continue;

                if(instruction.OpCode.Code == dnlib.DotNet.Emit.Code.Stfld) {
                    instruction.OpCode = OpCodes.Pop;
                    instruction.Operand = null;
                    instructions.Insert(i + 1, OpCodes.Pop.ToInstruction());
                    i++;
                    changes++;
                } else {
                    MelonLogger.Warning($"[Harmony] {method.FullName} reads {field.Name}, a HarmonyX-only field — that read cannot be translated.");
                }
            }
            method.Body.UpdateInstructionOffsets();
        }
        return changes;
    }

    private static int RetargetAttributes(ModuleDefMD module, Importer importer) {
        ConstructorInfo replacement = typeof(HarmonyPatch).GetConstructor([typeof(string), typeof(string), typeof(MethodType)]);
        if(replacement == null) return 0;

        int changes = 0;
        foreach(CustomAttribute attribute in AllCustomAttributes(module)) {
            IMethod constructor = attribute.Constructor;
            if(constructor == null || !IsHarmonyMember(constructor)) continue;
            if(Signature(constructor) != "HarmonyLib.HarmonyPatch::.ctor(System.String,System.String)") continue;

            attribute.Constructor = (ICustomAttributeType)importer.Import(replacement);
            attribute.ConstructorArguments.Add(new CAArgument(importer.ImportAsTypeSig(typeof(MethodType)), 0));
            changes++;
        }
        return changes;
    }

    private static IEnumerable<CustomAttribute> AllCustomAttributes(ModuleDefMD module) {
        foreach(CustomAttribute attribute in module.CustomAttributes) yield return attribute;
        if(module.Assembly != null)
            foreach(CustomAttribute attribute in module.Assembly.CustomAttributes) yield return attribute;
        foreach(TypeDef type in module.GetTypes()) {
            foreach(CustomAttribute attribute in type.CustomAttributes) yield return attribute;
            foreach(MethodDef method in type.Methods)
                foreach(CustomAttribute attribute in method.CustomAttributes) yield return attribute;
            foreach(FieldDef field in type.Fields)
                foreach(CustomAttribute attribute in field.CustomAttributes) yield return attribute;
            foreach(PropertyDef property in type.Properties)
                foreach(CustomAttribute attribute in property.CustomAttributes) yield return attribute;
        }
    }

    private static bool IsHarmonyMember(IMemberRef member) {
        ITypeDefOrRef declaring = member?.DeclaringType;
        return declaring is TypeRef typeRef && IsHarmonyScope(typeRef.ResolutionScope);
    }

    private static bool IsHarmonyScope(IResolutionScope scope) => scope switch {
        AssemblyRef assemblyRef => string.Equals(assemblyRef.Name, HarmonyAssemblyName, StringComparison.OrdinalIgnoreCase),
        TypeRef nested => IsHarmonyScope(nested.ResolutionScope),
        _ => false,
    };

    private static string Signature(IMethod method) =>
        $"{method.DeclaringType.FullName}::{method.Name}({string.Join(",", method.MethodSig.Params.Select(p => p.FullName).ToArray())})";

    public static void UnpatchOwn(Harmony harmony, MelonLogger.Instance logger) {
        if(harmony == null) return;
        try {
            harmony.UnpatchAll(harmony.Id);
        } catch(Exception e) {
            logger?.Error($"could not remove Harmony patches: {e}");
        }
    }
}
