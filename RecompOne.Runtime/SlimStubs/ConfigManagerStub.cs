namespace RecompOne.Runtime.Config;

/// <summary>
/// SWITCH_SLIM: zamiennik ConfigManager bez ImGui / paneli.
/// GameConfig pochodzi z istniejącego pliku w Config/.
/// </summary>
public static class ConfigManager
{
    public static GameConfig Game { get; } = new();
}
