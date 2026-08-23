namespace RecompOne.Runtime.Config;

public static class ConfigManager
{
    public static GameConfig Game { get; } = new();
}

public sealed class GameConfig
{
    public bool CatalogDiscovery { get; set; }
}
