namespace CrashBandicoot.Switch;

public sealed class SwitchGraphics
{
    private int _width;
    private int _height;

    public void Init(int width, int height)
    {
        _width = width;
        _height = height;
        Console.WriteLine($"[SwitchGraphics] Init {width}x{height}");
    }

    public void Present(ReadOnlySpan<byte> rgbaOrVramStub)
    {
        _ = rgbaOrVramStub;
        _ = _width;
        _ = _height;
    }

    public void Shutdown()
    {
        Console.WriteLine("[SwitchGraphics] Shutdown");
    }
}
