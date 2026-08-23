using System.Reflection;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Modding;

public sealed class ModInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Enabled { get; set; }
}

public static class ModLoader
{
    public static bool ReloadAssets() => false;
}

public static class ModManager
{
    public static IReadOnlyList<ModInfo> Mods { get; } = Array.Empty<ModInfo>();
    public static void Initialize() { }
    public static void Shutdown() { }
}

public static class HookManager
{
    public static void Commit() { }

    public static void AddPre(MethodInfo mi, Func<CpuContext, IMemory, bool> pre) { }

    public static void AddPost(MethodInfo mi, Action<CpuContext, IMemory> post) { }

    public static void AddPre(MethodInfo mi, Delegate pre) { }

    public static void AddPost(MethodInfo mi, Delegate post) { }
}
