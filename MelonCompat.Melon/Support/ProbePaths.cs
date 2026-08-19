namespace MelonLoader.Support;

// The assembly resolver lives in the bootstrap, which loaded this assembly and so
// cannot be referenced from it. The host hands over an adder instead, and this is
// where anything inside the shim reaches it.
public static class ProbePaths {
    private static Action<string> adder;

    public static void Bind(Action<string> add) => adder = add;

    public static void Add(string directory) {
        if(string.IsNullOrEmpty(directory)) return;
        adder?.Invoke(directory);
    }
}
