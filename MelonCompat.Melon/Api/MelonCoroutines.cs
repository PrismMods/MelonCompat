using System.Collections;
using MelonLoader.Support;

namespace MelonLoader;

// Coroutines need a MonoBehaviour. MelonLoader has one of its own; here they run
// on the host's persistent object, which outlives scene changes.
public static class MelonCoroutines {
    public static object Start(IEnumerator routine) => UnityHost.StartRoutine(routine);
    public static void Stop(object coroutineToken) => UnityHost.StopRoutine(coroutineToken);
}
