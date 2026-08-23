namespace RecompOne.Runtime.Cdrom;

public static class DiscOverlay
{
    public static bool TryOpen(string path, out Stream? stream)
    {
        stream = null;
        return false;
    }

    public static bool TryRead(string path, Span<byte> dest, out int read)
    {
        read = 0;
        return false;
    }

    public static void Clear() { }

    // dopasuj sygnatury do CueFs.cs gdy build pokaże CS1501
    public static object? Get(string path) => null;
    public static bool Has(string path) => false;
}
