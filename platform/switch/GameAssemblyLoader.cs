using System.Reflection;

namespace CrashBandicoot.Switch;

/// <summary>
/// Ładuje przygotowany na PC game.recomp.dll (nie commituj tego DLL do gita).
/// </summary>
public static class GameAssemblyLoader
{
    public static string? FindGameDll(string root)
    {
        var gameRoot = Path.Combine(root, "game");
        var candidates = new List<string>();

        void AddIfExists(string p)
        {
            if (File.Exists(p))
                candidates.Add(p);
        }

        AddIfExists(Path.Combine(gameRoot, "game.recomp.dll"));
        AddIfExists(Path.Combine(root, "game.recomp.dll"));

        if (Directory.Exists(gameRoot))
        {
            foreach (var dir in Directory.GetDirectories(gameRoot))
            {
                AddIfExists(Path.Combine(dir, "game.recomp.dll"));
                // inne możliwe nazwy z pipeline
                foreach (var f in Directory.GetFiles(dir, "*.dll"))
                {
                    var name = Path.GetFileName(f);
                    if (name.Contains("recomp", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("game", StringComparison.OrdinalIgnoreCase))
                        candidates.Add(f);
                }
            }
        }

        // unikalne, preferuj *recomp*
        candidates = candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var preferred = candidates.FirstOrDefault(c =>
            Path.GetFileName(c).Contains("recomp", StringComparison.OrdinalIgnoreCase));
        return preferred ?? candidates.FirstOrDefault();
    }

    public static void Inspect(string dllPath)
    {
        Console.WriteLine($"[Switch] Loading game assembly: {dllPath}");
        var asm = Assembly.LoadFrom(dllPath);
        Console.WriteLine($"[Switch] Assembly: {asm.FullName}");

        Type? entryType = null;
        foreach (var t in asm.GetExportedTypes())
        {
            if (t.Name is "Entry" or "Program")
            {
                Console.WriteLine($"[Switch] type: {t.FullName}");
                entryType = entryType ?? t;
            }
        }

        if (entryType is null)
        {
            Console.WriteLine("[Switch] Brak typu Entry/Program — wypisuję kilka typów:");
            foreach (var t in asm.GetExportedTypes().Take(20))
                Console.WriteLine($"  {t.FullName}");
            return;
        }

        foreach (var m in entryType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            Console.WriteLine($"[Switch] method: {entryType.Name}.{m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");

        // NIE wywołuj jeszcze Run — najpierw Runtime + host
        Console.WriteLine("[Switch] Inspect OK — Entry.Run podłączymy po IRuntimePlatformHost + Runtime.");
    }
}
