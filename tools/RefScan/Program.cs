using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

// Reports what a mod expects from an assembly, and which of it a stand-in does not
// provide. Comparison is by full signature, not just name: the interesting gaps are
// overloads that exist in HarmonyX and not in Harmony, which a name-only check
// silently passes.
//
//   refscan <mod.dll> <provider.dll> [assembly-name]
if (args.Length < 2) {
    Console.Error.WriteLine("usage: refscan <mod.dll> <provider.dll> [assembly-name]");
    return 2;
}

string modPath = args[0], providerPath = args[1];
string wanted = args.Length > 2 ? args[2] : "MelonLoader";

var (types, members) = ReadRequirements(modPath, wanted);
var provided = ReadProvided(providerPath);

var missingTypes = types.Where(t => !provided.Types.Contains(t)).OrderBy(t => t).ToArray();
var missingMembers = members
    .Where(m => !provided.Members.Contains(m))
    .Where(m => !missingTypes.Contains(m[..m.IndexOf("::", StringComparison.Ordinal)]))
    .OrderBy(m => m)
    .ToArray();

Console.WriteLine($"{Path.GetFileName(modPath)}: {types.Count} {wanted} types, {members.Count} members");
foreach (var t in missingTypes) Console.WriteLine($"  MISSING TYPE   {t}");
foreach (var m in missingMembers) Console.WriteLine($"  MISSING MEMBER {m}");
if (missingTypes.Length == 0 && missingMembers.Length == 0) Console.WriteLine("  fully provided");
return missingTypes.Length + missingMembers.Length == 0 ? 0 : 1;

static (HashSet<string> Types, HashSet<string> Members) ReadRequirements(string path, string wanted) {
    using var fs = File.OpenRead(path);
    using var pe = new PEReader(fs);
    if (!pe.HasMetadata) return ([], []);
    var md = pe.GetMetadataReader();
    var sig = new Sig(md);

    bool FromWanted(EntityHandle scope) => scope.Kind switch {
        HandleKind.AssemblyReference => md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)scope).Name) == wanted,
        HandleKind.TypeReference => FromWanted(md.GetTypeReference((TypeReferenceHandle)scope).ResolutionScope),
        _ => false,
    };

    string Name(TypeReferenceHandle h) {
        var tr = md.GetTypeReference(h);
        string n = md.GetString(tr.Name);
        if (tr.ResolutionScope.Kind == HandleKind.TypeReference) return Name((TypeReferenceHandle)tr.ResolutionScope) + "/" + n;
        string ns = md.GetString(tr.Namespace);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }

    HashSet<string> types = [], members = [];
    foreach (var h in md.TypeReferences)
        if (FromWanted(md.GetTypeReference(h).ResolutionScope)) types.Add(Name(h));

    foreach (var h in md.MemberReferences) {
        var mr = md.GetMemberReference(h);
        if (mr.Parent.Kind != HandleKind.TypeReference) continue;
        var parent = (TypeReferenceHandle)mr.Parent;
        if (!FromWanted(md.GetTypeReference(parent).ResolutionScope)) continue;

        string name = md.GetString(mr.Name);
        string parameters = mr.GetKind() == MemberReferenceKind.Method
            ? "(" + string.Join(",", mr.DecodeMethodSignature(sig, null).ParameterTypes) + ")"
            : "";
        members.Add($"{Name(parent)}::{name}{parameters}");
    }
    return (types, members);
}

// Inherited members count: a call to MelonMod::OnUpdate emits a reference to
// MelonMod even though MelonBase declares it, so every type is credited with
// everything up its own base chain.
static (HashSet<string> Types, HashSet<string> Members) ReadProvided(string path) {
    using var fs = File.OpenRead(path);
    using var pe = new PEReader(fs);
    var md = pe.GetMetadataReader();
    var sig = new Sig(md);

    string Name(TypeDefinition td) {
        string n = md.GetString(td.Name);
        if (td.IsNested) return Name(md.GetTypeDefinition(td.GetDeclaringType())) + "/" + n;
        string ns = md.GetString(td.Namespace);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }

    HashSet<string> types = [], members = [];
    foreach (var h in md.TypeDefinitions) {
        var td = md.GetTypeDefinition(h);
        string name = Name(td);
        types.Add(name);

        var current = td;
        var seen = new HashSet<int>();
        while (true) {
            foreach (var mh in current.GetMethods()) {
                var m = md.GetMethodDefinition(mh);
                var s = m.DecodeSignature(sig, null);
                members.Add($"{name}::{md.GetString(m.Name)}({string.Join(",", s.ParameterTypes)})");
            }
            foreach (var fh in current.GetFields())
                members.Add($"{name}::{md.GetString(md.GetFieldDefinition(fh).Name)}");

            if (current.BaseType.IsNil || current.BaseType.Kind != HandleKind.TypeDefinition) break;
            var b = (TypeDefinitionHandle)current.BaseType;
            if (!seen.Add(MetadataTokens.GetRowNumber(b))) break;
            current = md.GetTypeDefinition(b);
        }
    }
    return (types, members);
}

class Sig(MetadataReader reader) : ISignatureTypeProvider<string, object> {
    public string GetArrayType(string e, ArrayShape s) => e + "[]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> si) => "fnptr";
    public string GetGenericInstantiation(string g, ImmutableArray<string> a) => g + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object gc, int i) => "!!" + i;
    public string GetGenericTypeParameter(object gc, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool isRequired) => u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode t) => t.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte raw) {
        var td = r.GetTypeDefinition(h);
        var ns = r.GetString(td.Namespace);
        return string.IsNullOrEmpty(ns) ? r.GetString(td.Name) : ns + "." + r.GetString(td.Name);
    }
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte raw) {
        var tr = r.GetTypeReference(h);
        var ns = r.GetString(tr.Namespace);
        return string.IsNullOrEmpty(ns) ? r.GetString(tr.Name) : ns + "." + r.GetString(tr.Name);
    }
    public string GetTypeFromSpecification(MetadataReader r, object gc, TypeSpecificationHandle h, byte raw) =>
        r.GetTypeSpecification(h).DecodeSignature(this, gc);
}
