namespace RecompOne.Runtime.Host;

public static class InputManager
{
    /// <summary>
    /// Desktop: poll Silk. Slim: no-op — pad ustawia host Switch w Controller.*.
    /// </summary>
    public static void Poll() { }

    public static void SetRumble(byte large, byte small) { }
    public static void RequestFullscreenToggle() { }
    public static void RequestCheatMenuToggle() { }
    public static void RequestPauseMenuToggle() { }
}

public static class HostWindow
{
    public static void Initialize(string title) { }
    public static void Present(object? gpu) { }
    public static void Shutdown() { }
    public static void WaitForValidDisc() { }
    public static void SetEmbedParent(nint hwnd) { }
    public static void FitEmbeddedToParent() { }
    public static void SetFullscreen(bool on) { }
}

public static class Audio
{
    public static void Initialize() { }
    public static void SetMasterVolume(float v) { }
    public static void Attach(object? spu) { }
    public static void Shutdown() { }
}
