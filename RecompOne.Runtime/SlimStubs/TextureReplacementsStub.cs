namespace RecompOne.Runtime.Catalogs;

public static class TextureReplacements
{
    public static bool TryRegisterPng(string id, ReadOnlySpan<byte> png) => false;
    public static bool TryRegisterPngFile(string id, string path) => false;
    public static void Remove(string id) { }
    public static void Clear() { }

    public static bool TryApply(object? any) => false;
    public static bool ShouldReplace(object? any) => false;
}
