namespace RecompOne.Runtime.Config;

public static class ConfigManager
{
    public static GameConfig Game { get; } = new();
    public static ViewConfig View { get; } = new();

    public static void SaveView(object? panels = null) { }
}
