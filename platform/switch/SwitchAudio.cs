namespace CrashBandicoot.Switch;

public sealed class SwitchAudio
{
    private int _sampleRate;
    private int _channels;

    public void Init(int sampleRate = 44100, int channels = 2)
    {
        _sampleRate = sampleRate;
        _channels = channels;
        Console.WriteLine($"[SwitchAudio] Init {sampleRate} Hz, ch={channels}");
        // TODO: SDL_OpenAudioDevice / libnx audio
    }

    public void SubmitPcm(ReadOnlySpan<short> interleavedStereo)
    {
        _ = interleavedStereo;
        _ = _sampleRate;
        _ = _channels;
        // TODO: queue PCM
    }

    public void SetVolume(float volume01)
    {
        _ = volume01;
    }

    public void Shutdown()
    {
        Console.WriteLine("[SwitchAudio] Shutdown");
    }
}
