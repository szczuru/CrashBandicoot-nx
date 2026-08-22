namespace CrashBandicoot.Switch;

/// <summary>
/// Stub hosta Switch. Docelowo: IRuntimePlatformHost (Present/Audio/Init).
/// </summary>
public sealed class SwitchPlatformHost : IDisposable
{
    private bool _running;
    private readonly SwitchGraphics _graphics = new();
    private readonly SwitchAudio _audio = new();
    private readonly SwitchInput _input = new();

    public SwitchGraphics Graphics => _graphics;
    public SwitchAudio Audio => _audio;
    public SwitchInput Input => _input;

    public void Initialize()
    {
        Console.WriteLine("[SwitchHost] Initialize");
        _graphics.Init(1280, 720);
        _audio.Init(44100, 2);
        _running = true;
    }

    public void RunSmokeLoop(int seconds = 2)
    {
        var until = DateTime.UtcNow.AddSeconds(seconds);
        while (_running && DateTime.UtcNow < until)
        {
            _ = _input.ReadPad0();
            _graphics.Present(ReadOnlySpan<byte>.Empty);
            Thread.Sleep(16);
        }
    }

    public void Shutdown()
    {
        if (!_running)
            return;
        Console.WriteLine("[SwitchHost] Shutdown");
        _running = false;
        _audio.Shutdown();
        _graphics.Shutdown();
    }

    public void Dispose() => Shutdown();
}
