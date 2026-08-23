namespace RecompOne.Runtime.Modding;

public sealed class ModInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Enabled { get; set; }
}

public static class ModManager
{
    public static IReadOnlyList<ModInfo> Mods { get; } = Array.Empty<ModInfo>();

    public static void Initialize() { }

    public static void Shutdown() { }
}
