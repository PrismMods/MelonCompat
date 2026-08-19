using System.Collections;
using UnityEngine;

namespace MelonLoader.Support;

// MelonLoader hooks the engine itself; UnityModManager only offers per-mod update
// callbacks and an IMGUI panel. The missing pieces — a real OnGUI, OnApplicationQuit
// and a coroutine runner — come from a hidden MonoBehaviour that survives scene
// loads, which is what this is.
public sealed class UnityHost : MonoBehaviour {
    private static UnityHost instance;

    public static bool Active => instance != null;

    internal static Action OnUpdateCallback;
    internal static Action OnFixedUpdateCallback;
    internal static Action OnLateUpdateCallback;
    internal static Action OnGuiCallback;
    internal static Action OnQuitCallback;

    public static void Ensure() {
        if(instance != null) return;
        GameObject holder = new("MelonCompat") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(holder);
        instance = holder.AddComponent<UnityHost>();
    }

    public static void Destroy() {
        if(instance == null) return;
        GameObject holder = instance.gameObject;
        instance = null;
        Destroy(holder);
    }

    public static object StartRoutine(IEnumerator routine) {
        if(routine == null) return null;
        Ensure();
        return instance.StartCoroutine(routine);
    }

    public static void StopRoutine(object token) {
        if(instance == null || token == null) return;
        switch(token) {
            case Coroutine coroutine:
                instance.StopCoroutine(coroutine);
                break;
            case IEnumerator routine:
                instance.StopCoroutine(routine);
                break;
        }
    }

    private void Update() => OnUpdateCallback?.Invoke();
    private void FixedUpdate() => OnFixedUpdateCallback?.Invoke();
    private void LateUpdate() => OnLateUpdateCallback?.Invoke();
    private void OnGUI() => OnGuiCallback?.Invoke();
    private void OnApplicationQuit() => OnQuitCallback?.Invoke();
}
