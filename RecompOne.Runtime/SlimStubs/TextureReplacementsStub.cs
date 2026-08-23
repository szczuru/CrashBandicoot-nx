namespace RecompOne.Runtime.Catalogs;

public static class TextureReplacements
{
    public static bool HasAny => false;

    public static bool TryApply(int x, int y, int w, int h, ushort[]? pixels, int count)
        => false;

    public static bool TryRegisterPng(string id, ReadOnlySpan<byte> png) => false;
    public static bool TryRegisterPngFile(string id, string path) => false;
    public static void Remove(string id) { }
    public static void Clear() { }
}
