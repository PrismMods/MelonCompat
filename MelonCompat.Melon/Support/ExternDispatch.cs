using System.Reflection;

namespace MelonLoader.Support;

// A patch method's view of an engine internal call. The shapes are deliberately
// narrow — a value coming back, and the chance to replace it — because that is
// what patching a body-less property getter can actually mean.
public delegate bool ExternPrefix<T>(ref T __result);
public delegate void ExternPostfix<T>(ref T __result);

// The stand-in that redirected call sites land on.
//
// UnityEngine.Screen.width and friends are internal calls with no managed body, so
// Harmony cannot patch them: there is nothing to wrap. What it can do is rewrite
// every *caller*, and that is what happens here — the call site is transpiled to
// call Get<T>(id) instead, and Get runs the mod's prefixes, the real engine
// property, then the mod's postfixes.
//
// Get is on the hot path: Time.deltaTime alone is read from 200-odd places, many
// of them per frame. So it allocates nothing, takes no locks, and holds the patch
// lists as arrays that are swapped wholesale rather than mutated.
public static class ExternDispatch {
    private interface ISlot { }

    private static ISlot[] slots = [];

    private sealed class Slot<T> : ISlot {
        public Func<T> Original;
        public ExternPrefix<T>[] Prefixes = [];
        public ExternPostfix<T>[] Postfixes = [];

        public T Invoke() {
            T result = default;

            // A prefix returning false means "the engine value is not wanted",
            // which is exactly how Harmony defines skipping the original.
            ExternPrefix<T>[] prefixes = Prefixes;
            for(int i = 0; i < prefixes.Length; i++)
                if(!prefixes[i](ref result))
                    return result;

            result = Original();

            ExternPostfix<T>[] postfixes = Postfixes;
            for(int i = 0; i < postfixes.Length; i++) postfixes[i](ref result);
            return result;
        }
    }

    // Exceptions are deliberately not caught: Harmony does not catch them either,
    // and a try/catch on a per-frame path costs more than it is worth.
    public static T Get<T>(int id) => ((Slot<T>)slots[id]).Invoke();

    public static MethodInfo DispatchMethod(Type valueType) =>
        typeof(ExternDispatch).GetMethod(nameof(Get), BindingFlags.Public | BindingFlags.Static)
            .MakeGenericMethod(valueType);

    internal static int CreateSlot(Type valueType, MethodBase original) {
        Type slotType = typeof(Slot<>).MakeGenericType(valueType);
        object slot = Activator.CreateInstance(slotType);
        slotType.GetField(nameof(Slot<int>.Original))
            .SetValue(slot, Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(valueType), (MethodInfo)original));

        ISlot[] grown = new ISlot[slots.Length + 1];
        Array.Copy(slots, grown, slots.Length);
        grown[slots.Length] = (ISlot)slot;
        slots = grown;
        return grown.Length - 1;
    }

    internal static void AddPatch(int id, Type valueType, MethodInfo patch, bool isPrefix) {
        object slot = slots[id];
        Type slotType = slot.GetType();
        string fieldName = isPrefix ? nameof(Slot<int>.Prefixes) : nameof(Slot<int>.Postfixes);
        FieldInfo field = slotType.GetField(fieldName);

        Type delegateType = (isPrefix ? typeof(ExternPrefix<>) : typeof(ExternPostfix<>)).MakeGenericType(valueType);
        Delegate created = Delegate.CreateDelegate(delegateType, patch);

        Array existing = (Array)field.GetValue(slot);
        Array grown = Array.CreateInstance(delegateType, existing.Length + 1);
        Array.Copy(existing, grown, existing.Length);
        grown.SetValue(created, existing.Length);
        field.SetValue(slot, grown);
    }
}
