namespace CrashBandicoot.Switch;

/// <summary>
/// Present VRAM / GLES. Na start software blit; później GPU HLE z Runtime.
/// </summary>
public sealed class SwitchGraphics
{
    private int _width;
    private int _height;

    public void Init(int width, int height)
    {
        _width = width;
        _height = height;
        Console.WriteLine($"[SwitchGraphics] Init {width}x{height}");
        // TODO: SDL2 / libnx EGL + texture
    }

    public void Present(ReadOnlySpan<byte> rgbaOrVramStub)
    {
        // TODO: upload texture + swap buffers
        _ = rgbaOrVramStub;
        _ = _width;
        _ = _height;
    }

    public void Shutdown()
    {
        Console.WriteLine("[SwitchGraphics] Shutdown");
    }
}
