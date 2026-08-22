using System.Reflection;

namespace CrashBandicoot.Switch;

/// <summary>
/// Ładuje przygotowany na PC game.recomp.dll.
/// Wymaga RecompOne.Runtime.dll w /switch/ lub obok game DLL.
/// </summary>
public static class GameAssemblyLoader
{
    public static string? FindGameDll(string root)
    {
        var gameRoot = Path.Combine(root, "game");
        var candidates = new List<string>();

        void AddIfExists(string p)
        {
            try
            {
                if (File.Exists(p))
                    candidates.Add(p);
            }
            catch
            {
                // ignore
            }
        }

        AddIfExists(Path.Combine(gameRoot, "game.recomp.dll"));
        AddIfExists(Path.Combine(root, "game.recomp.dll"));

        try
        {
            if (Directory.Exists(gameRoot))
            {
                foreach (var dir in Directory.GetDirectories(gameRoot))
                {
                    AddIfExists(Path.Combine(dir, "game.recomp.dll"));
                    foreach (var f in Directory.GetFiles(dir, "*.dll"))
                    {
                        var name = Path.GetFileName(f);
                        if (name.Contains("recomp", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("CrashBandicoot.Game.dll", StringComparison.OrdinalIgnoreCase))
                            candidates.Add(f);
                    }
                }

                foreach (var f in Directory.GetFiles(gameRoot, "*.dll"))
                {
                    var name = Path.GetFileName(f);
                    if (name.Contains("recomp", StringComparison.OrdinalIgnoreCase))
                        candidates.Add(f);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] FindGameDll scan error: {ex.Message}");
        }

        candidates = candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var preferred = candidates.FirstOrDefault(c =>
            Path.GetFileName(c).Contains("recomp", StringComparison.OrdinalIgnoreCase));

        return preferred ?? candidates.FirstOrDefault();
    }

    public static void LogDependencyHints(string root, string gameDllPath)
    {
        var dirs = new[]
        {
            root,
            Path.GetDirectoryName(gameDllPath) ?? root,
            Path.Combine(root, "game"),
        };

        Console.WriteLine("[Switch] Dependency probe (szukam RecompOne.Runtime.dll):");
        foreach (var dir in dirs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var runtime = Path.Combine(dir, "RecompOne.Runtime.dll");
            var exists = false;
            try { exists = File.Exists(runtime); } catch { /* ignore */ }
            Console.WriteLine($"  {runtime} exists={exists}");
        }
    }

    public static void Inspect(string dllPath)
    {
        Console.WriteLine($"[Switch] Loading game assembly: {dllPath}");
        LogDependencyHints(
            Path.GetDirectoryName(dllPath) is { } g && g.Contains("game")
                ? Path.GetFullPath(Path.Combine(g, "..", ".."))
                : "/switch",
            dllPath);

        // Prostsze: probe względem CWD i folderu gry
        try
        {
            var cwd = Directory.GetCurrentDirectory();
            LogDependencyHints(cwd, dllPath);
        }
        catch
        {
            // ignore
        }

        Assembly asm;
        try
        {
            asm = Assembly.LoadFrom(dllPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] Assembly.LoadFrom failed: {ex}");
            throw;
        }

        Console.WriteLine($"[Switch] Assembly: {asm.FullName}");

        Type[] types;
        try
        {
            types = asm.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException rtle)
        {
            Console.WriteLine("[Switch] ReflectionTypeLoadException — brakujące zależności:");
            if (rtle.LoaderExceptions != null)
            {
                foreach (var le in rtle.LoaderExceptions)
                {
                    if (le != null)
                        Console.WriteLine($"  LOADER: {le.GetType().Name}: {le.Message}");
                }
            }

            types = rtle.Types?.Where(t => t != null).Cast<Type>().ToArray() ?? Array.Empty<Type>();
            Console.WriteLine($"[Switch] Udało się odczytać częściowo typów: {types.Length}");
        }

        Type? entryType = null;
        foreach (var t in types)
        {
            if (t.Name is "Entry" or "Program")
            {
                Console.WriteLine($"[Switch] type: {t.FullName}");
                entryType ??= t;
            }
        }

        if (entryType is null)
        {
            Console.WriteLine("[Switch] Brak Entry/Program w załadowanych typach. Sample:");
            foreach (var t in types.Take(30))
                Console.WriteLine($"  {t.FullName}");
            return;
        }

        foreach (var m in entryType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var args = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
            Console.WriteLine($"[Switch] method: {entryType.Name}.{m.Name}({args})");
        }

        Console.WriteLine("[Switch] Inspect OK.");
    }
}
