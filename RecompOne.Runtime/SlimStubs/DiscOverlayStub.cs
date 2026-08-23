namespace RecompOne.Runtime.Cdrom;

public static class DiscOverlay
{
    public static bool TryReadFile(string path, out byte[] overlay)
    {
        overlay = Array.Empty<byte>();
        return false;
    }

    public static bool TryLocate(string name, out int lba, out uint size)
    {
        lba = 0;
        size = 0;
        return false;
    }

    public static bool TryReadSectorData(int lba, int size, out byte[] overlay)
    {
        overlay = Array.Empty<byte>();
        return false;
    }
}
