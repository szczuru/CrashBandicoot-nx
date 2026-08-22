namespace CrashBandicoot.Switch;

public sealed class SwitchAudio
{
    private int _sampleRate;
    private int _channels;
    private float _volume = 1f;

    public void Init(int sampleRate = 44100, int channels = 2)
    {
        _sampleRate = sampleRate;
        _channels = channels;
        Console.WriteLine($"[SwitchAudio] Init {sampleRate} Hz, ch={channels}");
    }

    public void SubmitPcm(ReadOnlySpan<short> interleavedStereo)
    {
        _ = interleavedStereo;
        _ = _sampleRate;
        _ = _channels;
        _ = _volume;
    }

    public void SetVolume(float volume01)
    {
        _volume = Math.Clamp(volume01, 0f, 1f);
    }

    public void Shutdown()
    {
        Console.WriteLine("[SwitchAudio] Shutdown");
    }
}
