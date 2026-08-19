using System.Linq;
using System.Reflection;

namespace MelonLoader;

// A priority-ordered subscriber list. Subscription order is not enough for mods
// that need to run before or after each other, and a subscriber that throws must
// not stop the rest of the list, so invocation catches per-subscriber.
public class MelonEventBase<T> where T : Delegate {
    public class MelonEventSubscriber {
        public MelonAssembly melonAssembly;
        public T del;
        public int priority;
        public bool unsubscribeOnFirstInvocation;
    }

    private readonly List<MelonEventSubscriber> subscribers = [];
    public readonly bool oneTimeUse;
    private bool invoked;

    public bool Disposed { get; private set; }

    public MelonEventBase(bool oneTimeUse = false) => this.oneTimeUse = oneTimeUse;

    public MelonEventSubscriber[] GetSubscribers() {
        lock(subscribers) return subscribers.ToArray();
    }

    public void Subscribe(T action, int priority = 0, bool unsubscribeOnFirstInvocation = false) {
        if(action == null || Disposed) return;
        lock(subscribers) {
            if(CheckIfSubscribedCore(action.Method, action.Target)) return;
            subscribers.Add(new MelonEventSubscriber {
                del = action,
                priority = priority,
                unsubscribeOnFirstInvocation = unsubscribeOnFirstInvocation,
                melonAssembly = MelonAssembly.GetMelonAssemblyOfMember(action.Method, action.Target),
            });
            subscribers.Sort((a, b) => a.priority.CompareTo(b.priority));
        }
        // A one-time event that has already fired still owes late subscribers the
        // call they signed up for — this is what makes OnApplicationStart usable
        // from a mod that registered after startup.
        if(oneTimeUse && invoked) {
            SafeInvoke(action);
            Unsubscribe(action);
        }
    }

    public void Unsubscribe(T action) {
        if(action == null) return;
        Unsubscribe(action.Method, action.Target);
    }

    public void Unsubscribe(MethodInfo method, object target = null) {
        lock(subscribers) subscribers.RemoveAll(s => s.del.Method == method && ReferenceEquals(s.del.Target, target));
    }

    public void UnsubscribeAll() {
        lock(subscribers) subscribers.Clear();
    }

    public bool CheckIfSubscribed(MethodInfo method, object target = null) {
        lock(subscribers) return CheckIfSubscribedCore(method, target);
    }

    private bool CheckIfSubscribedCore(MethodInfo method, object target) =>
        subscribers.Any(s => s.del.Method == method && ReferenceEquals(s.del.Target, target));

    public void Invoke(Action<T> delegateInvoker) {
        if(Disposed) return;
        MelonEventSubscriber[] snapshot;
        lock(subscribers) {
            // These fire every frame. Copying an empty list 60 times a second is
            // pure garbage, so the common case leaves early.
            if(subscribers.Count == 0) {
                invoked = true;
                return;
            }
            snapshot = subscribers.ToArray();
        }
        invoked = true;
        foreach(MelonEventSubscriber subscriber in snapshot) {
            try {
                delegateInvoker(subscriber.del);
            } catch(Exception e) {
                MelonLogger.Error($"an event subscriber from {subscriber.melonAssembly?.Assembly?.GetName().Name ?? "an unknown assembly"} threw: {e}");
            }
            if(subscriber.unsubscribeOnFirstInvocation) Unsubscribe(subscriber.del);
        }
        if(oneTimeUse) UnsubscribeAll();
    }

    private void SafeInvoke(T action) {
        try {
            action.DynamicInvoke();
        } catch(Exception e) {
            MelonLogger.Error($"a late event subscriber threw: {e}");
        }
    }

    public void Dispose() {
        UnsubscribeAll();
        Disposed = true;
    }
}

public class MelonEvent : MelonEventBase<LemonAction> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke() => Invoke(d => d());
}

public class MelonEvent<T1> : MelonEventBase<LemonAction<T1>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1) => Invoke(d => d(arg1));
}

public class MelonEvent<T1, T2> : MelonEventBase<LemonAction<T1, T2>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2) => Invoke(d => d(arg1, arg2));
}

public class MelonEvent<T1, T2, T3> : MelonEventBase<LemonAction<T1, T2, T3>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3) => Invoke(d => d(arg1, arg2, arg3));
}

public class MelonEvent<T1, T2, T3, T4> : MelonEventBase<LemonAction<T1, T2, T3, T4>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Invoke(d => d(arg1, arg2, arg3, arg4));
}

public class MelonEvent<T1, T2, T3, T4, T5> : MelonEventBase<LemonAction<T1, T2, T3, T4, T5>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => Invoke(d => d(arg1, arg2, arg3, arg4, arg5));
}

public class MelonEvent<T1, T2, T3, T4, T5, T6> : MelonEventBase<LemonAction<T1, T2, T3, T4, T5, T6>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => Invoke(d => d(arg1, arg2, arg3, arg4, arg5, arg6));
}

public class MelonEvent<T1, T2, T3, T4, T5, T6, T7> : MelonEventBase<LemonAction<T1, T2, T3, T4, T5, T6, T7>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => Invoke(d => d(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
}

public class MelonEvent<T1, T2, T3, T4, T5, T6, T7, T8> : MelonEventBase<LemonAction<T1, T2, T3, T4, T5, T6, T7, T8>> {
    public MelonEvent(bool oneTimeUse = false) : base(oneTimeUse) { }
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => Invoke(d => d(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
}
